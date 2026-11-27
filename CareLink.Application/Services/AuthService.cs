using System.Text.RegularExpressions;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.DTOs.SecurityEvents;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Application.Services;

public class AuthService : IAuthService
{
    private static readonly Regex EmailFormatRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;
    private readonly ISecurityEventService _securityEventService;
    private readonly IPermissionService _permissionService;
    private readonly IOrganizationDocumentStorageService _documentStorageService;
    private readonly IVerificationService? _verificationService;
    private readonly IAuditLogService? _auditLogService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger,
        ISecurityEventService securityEventService,
        IPermissionService permissionService,
        IOrganizationDocumentStorageService? documentStorageService = null,
        IVerificationService? verificationService = null,
        IAuditLogService? auditLogService = null)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _memoryCache = memoryCache;
        _logger = logger;
        _securityEventService = securityEventService;
        _permissionService = permissionService;
        _documentStorageService = documentStorageService ?? new OrganizationDocumentStorageService();
        _verificationService = verificationService;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var rawRequestedRole = string.IsNullOrWhiteSpace(request.Role)
            ? UserRoles.Citizen
            : request.Role.Trim();

        if (!UserRoles.IsPublicRegistrationRole(rawRequestedRole))
        {
            return ApiResponse<RegisterResponseDto>.FailureResult(
                "Only Citizen, Volunteer, and Partner Organization accounts can be registered publicly. Internal roles must be created by an authorized administrator.");
        }

        var assignedRole = UserRoles.NormalizeRoleName(rawRequestedRole);
        var isPartnerRegistration = string.Equals(assignedRole, UserRoles.PartnerOfficer, StringComparison.OrdinalIgnoreCase);
        var isVolunteerRegistration = string.Equals(assignedRole, UserRoles.Volunteer, StringComparison.OrdinalIgnoreCase);

        // Resolve representative full name from RepresentativeFirstName/LastName or FullName
        var repFirstName = request.RepresentativeFirstName?.Trim();
        var repLastName = request.RepresentativeLastName?.Trim();
        var fullName = request.FullName?.Trim();

        if (string.IsNullOrWhiteSpace(fullName) &&
            (!string.IsNullOrWhiteSpace(repFirstName) || !string.IsNullOrWhiteSpace(repLastName)))
        {
            fullName = $"{repFirstName} {repLastName}".Trim();
        }
        else if (!string.IsNullOrWhiteSpace(fullName) &&
                 string.IsNullOrWhiteSpace(repFirstName) &&
                 string.IsNullOrWhiteSpace(repLastName))
        {
            var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            repFirstName = parts.Length > 0 ? parts[0] : fullName;
            repLastName = parts.Length > 1 ? parts[1] : string.Empty;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ApiResponse<RegisterResponseDto>.FailureResult("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<RegisterResponseDto>.FailureResult("Email address is required.");
        }

        var email = request.Email.Trim();
        if (!EmailFormatRegex.IsMatch(email))
        {
            return ApiResponse<RegisterResponseDto>.FailureResult("Please provide a valid login email address.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return ApiResponse<RegisterResponseDto>.FailureResult("Password must be at least 6 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.ConfirmPassword) && request.Password != request.ConfirmPassword)
        {
            return ApiResponse<RegisterResponseDto>.FailureResult("Passwords do not match.");
        }

        var contactPhone = !string.IsNullOrWhiteSpace(request.RepresentativePhone)
            ? request.RepresentativePhone.Trim()
            : request.PhoneNumber?.Trim();

        if (string.IsNullOrWhiteSpace(contactPhone))
        {
            return ApiResponse<RegisterResponseDto>.FailureResult("Phone number is required.");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        var existingByName = existingByEmail ?? await _userManager.FindByNameAsync(email);
        if (existingByName != null)
        {
            return ApiResponse<RegisterResponseDto>.FailureResult(
                "An account with this email address already exists.");
        }

        Organization? partnerOrg = null;
        var storedDocuments = new List<OrganizationDocument>();

        if (isPartnerRegistration)
        {
            var validationErrors = ValidatePartnerOrganizationRegistration(request, repFirstName, repLastName, contactPhone);
            if (validationErrors.Count > 0)
            {
                return ApiResponse<RegisterResponseDto>.FailureResult(
                    validationErrors[0],
                    validationErrors);
            }

            if (!string.IsNullOrWhiteSpace(request.OrganizationName))
            {
                var normalizedOrgName = request.OrganizationName.Trim().ToLower();
                var duplicateOrgName = await _userManager.Users
                    .Include(u => u.Organization)
                    .AnyAsync(u => u.Organization != null && u.Organization.Name.ToLower() == normalizedOrgName);

                if (duplicateOrgName)
                {
                    return ApiResponse<RegisterResponseDto>.FailureResult(
                        $"A Partner Organisation with the name '{request.OrganizationName.Trim()}' is already registered.");
                }

                if (!string.IsNullOrWhiteSpace(request.RegistrationNumber))
                {
                    var normalizedRegNum = request.RegistrationNumber.Trim().ToLower();
                    var duplicateRegNo = await _userManager.Users
                        .Include(u => u.Organization)
                        .AnyAsync(u => u.Organization != null &&
                                       u.Organization.RegistrationNumber != null &&
                                       u.Organization.RegistrationNumber.ToLower() == normalizedRegNum);

                    if (duplicateRegNo)
                    {
                        return ApiResponse<RegisterResponseDto>.FailureResult(
                            $"A Partner Organisation with registration number '{request.RegistrationNumber.Trim()}' is already registered.");
                    }
                }

                var orgId = Guid.NewGuid();

                if (request.Documents != null && request.Documents.Count > 0)
                {
                    foreach (var docDto in request.Documents)
                    {
                        var (isValidDoc, docError) = _documentStorageService.ValidateDocument(docDto);
                        if (!isValidDoc)
                        {
                            return ApiResponse<RegisterResponseDto>.FailureResult(
                                docError ?? "Invalid verification document uploaded.");
                        }

                        var storedDoc = await _documentStorageService.StoreDocumentAsync(orgId, docDto, email);
                        storedDocuments.Add(storedDoc);
                    }
                }

                var orgType = string.IsNullOrWhiteSpace(request.OrganizationType)
                    ? "Local NGO"
                    : request.OrganizationType.Trim();

                var orgEmail = !string.IsNullOrWhiteSpace(request.OrganizationEmail)
                    ? request.OrganizationEmail.Trim()
                    : email;

                var orgPhone = !string.IsNullOrWhiteSpace(request.OrganizationPhone)
                    ? request.OrganizationPhone.Trim()
                    : contactPhone;

                var orgAddress = !string.IsNullOrWhiteSpace(request.OrganizationAddress)
                    ? request.OrganizationAddress.Trim()
                    : request.Address?.Trim();

                var repEmail = !string.IsNullOrWhiteSpace(request.RepresentativeEmail)
                    ? request.RepresentativeEmail.Trim()
                    : email;

                partnerOrg = new Organization
                {
                    Id = orgId,
                    Name = request.OrganizationName.Trim(),
                    Type = orgType,
                    OtherOrganizationType = string.Equals(orgType, "Other", StringComparison.OrdinalIgnoreCase)
                        ? request.OtherOrganizationType?.Trim()
                        : null,
                    RegistrationCountry = string.IsNullOrWhiteSpace(request.RegistrationCountry)
                        ? "Sri Lanka"
                        : request.RegistrationCountry.Trim(),
                    RegistrationNumber = request.RegistrationNumber?.Trim(),
                    YearEstablished = request.YearEstablished,
                    Website = request.Website?.Trim(),
                    ContactEmail = orgEmail,
                    ContactPhone = orgPhone,
                    SecondaryContactPhone = request.SecondaryContactPhone?.Trim(),
                    Address = orgAddress,
                    District = request.District?.Trim(),
                    Province = request.Province?.Trim(),
                    Description = request.OrganizationDescription?.Trim(),
                    AreasOfOperation = request.AreasOfOperation != null && request.AreasOfOperation.Count > 0
                        ? string.Join(", ", request.AreasOfOperation.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()))
                        : null,
                    AreasOfAssistance = request.AreasOfAssistance != null && request.AreasOfAssistance.Count > 0
                        ? string.Join(", ", request.AreasOfAssistance.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()))
                        : null,
                    OtherAreaOfAssistance = request.OtherAreaOfAssistance?.Trim(),
                    OperationalCapacity = request.OperationalCapacity?.Trim(),
                    EmergencyResponseAvailability = request.EmergencyResponseAvailability?.Trim(),
                    AdditionalResources = request.AdditionalResources?.Trim(),
                    RepresentativeFirstName = repFirstName,
                    RepresentativeLastName = repLastName,
                    RepresentativeFullName = fullName,
                    RepresentativeDesignation = request.RepresentativeDesignation?.Trim(),
                    RepresentativeEmail = repEmail,
                    RepresentativePhone = contactPhone,
                    RepresentativeAlternativePhone = request.RepresentativeAlternativePhone?.Trim(),
                    ConfirmedAuthorisedRepresentative = request.ConfirmAuthorisedRepresentative ?? true,
                    ConfirmedAccurateInformation = request.ConfirmAccurateInformation ?? true,
                    AgreedToTermsAndPrivacy = request.AgreeTermsAndPrivacy ?? true,
                    IsActive = false,
                    ApprovalStatus = AccountStatuses.PendingApproval,
                    CreatedAt = DateTime.UtcNow,
                    Documents = storedDocuments
                };
            }
        }

        var accountStatus = isPartnerRegistration
            ? AccountStatuses.PendingApproval
            : AccountStatuses.Active;
        var isActiveNow = !isPartnerRegistration;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PhoneNumber = contactPhone,
            AlternativePhoneNumber = request.RepresentativeAlternativePhone?.Trim(),
            Designation = request.RepresentativeDesignation?.Trim(),
            NIC = request.NIC?.Trim(),
            Address = (!string.IsNullOrWhiteSpace(request.OrganizationAddress) ? request.OrganizationAddress.Trim() : request.Address?.Trim()),
            OrganizationId = partnerOrg?.Id ?? request.OrganizationId,
            Organization = partnerOrg,
            IsActive = isActiveNow,
            AccountStatus = accountStatus,
            EmailConfirmed = true,
            RequiresPasswordSetup = false,
            Skills = isVolunteerRegistration
                ? (string.IsNullOrWhiteSpace(request.Skills) ? "General Humanitarian Support" : request.Skills.Trim())
                : request.Skills?.Trim(),
            Availability = isVolunteerRegistration
                ? (string.IsNullOrWhiteSpace(request.Availability) ? "Immediate" : request.Availability.Trim())
                : request.Availability?.Trim(),
            PreferredLocation = isVolunteerRegistration
                ? (string.IsNullOrWhiteSpace(request.PreferredLocation) ? (request.Address?.Trim() ?? "Greater Riverton") : request.PreferredLocation.Trim())
                : request.PreferredLocation?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            var errorMessage = errors.Count > 0
                ? string.Join("; ", errors)
                : "User registration failed.";
            return ApiResponse<RegisterResponseDto>.FailureResult(errorMessage, errors);
        }

        if (partnerOrg != null)
        {
            partnerOrg.RepresentativeUserId = user.Id;
            await _userManager.UpdateAsync(user);
        }

        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(assignedRole));
        }

        await _userManager.AddToRoleAsync(user, assignedRole);

        string? verificationRequestId = null;
        if (isPartnerRegistration && partnerOrg != null && _verificationService != null)
        {
            try
            {
                var verReq = await _verificationService.RegisterPartnerOrganizationRequestAsync(partnerOrg, user);
                verificationRequestId = verReq.Id;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Non-fatal error creating verification queue record for '{OrgName}': {ErrorType}", partnerOrg.Name, ex.GetType().Name);
            }
        }

        if (isPartnerRegistration && partnerOrg != null && _auditLogService != null)
        {
            try
            {
                await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
                {
                    Action = "Partner organisation registration submitted",
                    Actor = $"{user.FullName} ({user.Email})",
                    Record = verificationRequestId ?? partnerOrg.Id.ToString(),
                    Change = $"New Partner Organisation '{partnerOrg.Name}' ({partnerOrg.Type}) → PendingApproval",
                    Rationale = $"Submitted with {storedDocuments.Count} verification document(s). Representative: {user.FullName} ({partnerOrg.RepresentativeDesignation ?? "Authorised Representative"}).",
                    Type = "Organisation",
                    Environment = "Production"
                });
            }
            catch
            {
                // Non-fatal
            }
        }

        var emailSent = false;
        var adminNotified = false;
        try
        {
            if (isPartnerRegistration)
            {
                var orgNameForEmail = partnerOrg?.Name ?? request.OrganizationName ?? "Partner Organisation";
                emailSent = await _emailService.SendPartnerPendingApprovalEmailAsync(
                    user.Email,
                    user.FullName,
                    orgNameForEmail);

                if (partnerOrg != null &&
                    !string.IsNullOrWhiteSpace(partnerOrg.RepresentativeEmail) &&
                    !string.Equals(partnerOrg.RepresentativeEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    await _emailService.SendPartnerPendingApprovalEmailAsync(
                        partnerOrg.RepresentativeEmail,
                        user.FullName,
                        orgNameForEmail);
                }

                // Notify authorised System Administrators
                var adminEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                {
                    adminEmails.Add(_emailSettings.SenderEmail.Trim());
                }

                try
                {
                    var sysAdmins = await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin);
                    var admins = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);
                    foreach (var adminUser in sysAdmins.Concat(admins))
                    {
                        if (adminUser.IsActive && !string.IsNullOrWhiteSpace(adminUser.Email))
                        {
                            adminEmails.Add(adminUser.Email.Trim());
                        }
                    }
                }
                catch
                {
                    // Ignore role query errors in isolated unit tests
                }

                foreach (var adminEmail in adminEmails)
                {
                    var sentToAdmin = await _emailService.SendAdminNewPartnerNotificationEmailAsync(
                        adminEmail,
                        orgNameForEmail,
                        partnerOrg?.Type ?? request.OrganizationType ?? "Partner NGO",
                        user.FullName,
                        partnerOrg?.RepresentativeEmail ?? user.Email);
                    if (sentToAdmin)
                    {
                        adminNotified = true;
                    }
                }
            }
            else if (isVolunteerRegistration)
            {
                emailSent = await _emailService.SendVolunteerWelcomeEmailAsync(
                    user.Email,
                    user.FullName,
                    user.Skills,
                    user.Availability);
            }
            else
            {
                emailSent = await _emailService.SendCitizenWelcomeEmailAsync(
                    user.Email,
                    user.FullName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Non-fatal error while sending registration email to {Email}: {ErrorType}", user.Email, ex.GetType().Name);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = isActiveNow
            ? (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList()
            : new List<string>();

        string? token = null;
        DateTime? expiresAt = null;
        string responseMessage;

        if (isPartnerRegistration)
        {
            responseMessage = "Your CareLink Partner Organisation registration has been received and is awaiting administrator review.";
        }
        else
        {
            token = _jwtTokenGenerator.GenerateToken(user, roles);
            expiresAt = DateTime.UtcNow.AddHours(24);
            responseMessage = isVolunteerRegistration
                ? "Volunteer registration completed successfully. Your account is active immediately."
                : "Citizen registration completed successfully. Your account is active immediately.";
        }

        var responseDto = new RegisterResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AccountStatus = user.AccountStatus,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            RequiresEmailVerification = false,
            RequiresAdminApproval = isPartnerRegistration,
            OrganizationId = partnerOrg?.Id ?? user.OrganizationId,
            OrganizationName = partnerOrg?.Name,
            VerificationRequestId = verificationRequestId,
            EmailSent = emailSent,
            AdminNotificationSent = adminNotified,
            UploadedDocumentsCount = storedDocuments.Count,
            Token = token,
            ExpiresAt = expiresAt,
            Message = responseMessage,
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<RegisterResponseDto>.SuccessResult(responseDto, responseDto.Message);
    }

    private List<string> ValidatePartnerOrganizationRegistration(
        RegisterRequestDto request,
        string? repFirstName,
        string? repLastName,
        string? contactPhone)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.OrganizationName) && !request.OrganizationId.HasValue)
        {
            errors.Add("Organisation name is required for Partner Organisation registration.");
            return errors;
        }

        if (request.OrganizationId.HasValue && string.IsNullOrWhiteSpace(request.OrganizationName))
        {
            return errors;
        }

        // Check if this is an extended 3-step partner registration payload
        var hasExtendedPayload =
            !string.IsNullOrWhiteSpace(request.RegistrationCountry) ||
            !string.IsNullOrWhiteSpace(request.OrganizationEmail) ||
            !string.IsNullOrWhiteSpace(request.OrganizationPhone) ||
            !string.IsNullOrWhiteSpace(request.OrganizationAddress) ||
            !string.IsNullOrWhiteSpace(request.District) ||
            !string.IsNullOrWhiteSpace(request.Province) ||
            !string.IsNullOrWhiteSpace(request.OperationalCapacity) ||
            !string.IsNullOrWhiteSpace(request.EmergencyResponseAvailability) ||
            !string.IsNullOrWhiteSpace(request.RepresentativeDesignation) ||
            !string.IsNullOrWhiteSpace(request.RepresentativeEmail) ||
            (request.AreasOfOperation != null && request.AreasOfOperation.Count > 0) ||
            (request.AreasOfAssistance != null && request.AreasOfAssistance.Count > 0) ||
            (request.Documents != null && request.Documents.Count > 0) ||
            request.ConfirmAuthorisedRepresentative.HasValue ||
            request.ConfirmAccurateInformation.HasValue ||
            request.AgreeTermsAndPrivacy.HasValue;

        if (string.IsNullOrWhiteSpace(request.OrganizationType))
        {
            errors.Add("Organisation type is required.");
        }
        else if (string.Equals(request.OrganizationType.Trim(), "Other", StringComparison.OrdinalIgnoreCase) &&
                 string.IsNullOrWhiteSpace(request.OtherOrganizationType))
        {
            errors.Add("Please specify your organisation type when 'Other' is selected.");
        }

        if (request.YearEstablished.HasValue &&
            (request.YearEstablished.Value < 1800 || request.YearEstablished.Value > DateTime.UtcNow.Year))
        {
            errors.Add($"Year established must be between 1800 and {DateTime.UtcNow.Year}.");
        }

        if (!string.IsNullOrWhiteSpace(request.OrganizationEmail) &&
            !EmailFormatRegex.IsMatch(request.OrganizationEmail.Trim()))
        {
            errors.Add("Please enter a valid official organisation email address.");
        }

        if (!string.IsNullOrWhiteSpace(request.RepresentativeEmail) &&
            !EmailFormatRegex.IsMatch(request.RepresentativeEmail.Trim()))
        {
            errors.Add("Please enter a valid authorised representative contact email address.");
        }

        if (!hasExtendedPayload)
        {
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.RegistrationCountry))
        {
            errors.Add("Registration country is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationEmail))
        {
            errors.Add("Official organisation email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationPhone))
        {
            errors.Add("Official organisation contact number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationAddress) && string.IsNullOrWhiteSpace(request.Address))
        {
            errors.Add("Organisation address is required.");
        }

        if (string.IsNullOrWhiteSpace(request.District))
        {
            errors.Add("District is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Province))
        {
            errors.Add("Province is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationDescription))
        {
            errors.Add("Operational scope / description is required.");
        }

        if (request.AreasOfOperation == null || request.AreasOfOperation.Count(a => !string.IsNullOrWhiteSpace(a)) == 0)
        {
            errors.Add("Please select at least one main area of operation.");
        }

        if (request.AreasOfAssistance == null || request.AreasOfAssistance.Count(a => !string.IsNullOrWhiteSpace(a)) == 0)
        {
            errors.Add("Please select at least one main area of humanitarian assistance.");
        }
        else if (request.AreasOfAssistance.Any(a => string.Equals(a?.Trim(), "Other", StringComparison.OrdinalIgnoreCase)) &&
                 string.IsNullOrWhiteSpace(request.OtherAreaOfAssistance))
        {
            errors.Add("Please describe your other area of assistance when 'Other' is selected.");
        }

        if (string.IsNullOrWhiteSpace(request.OperationalCapacity))
        {
            errors.Add("Operational capacity / description is required.");
        }

        if (string.IsNullOrWhiteSpace(request.EmergencyResponseAvailability))
        {
            errors.Add("Emergency response availability is required.");
        }

        if (string.IsNullOrWhiteSpace(repFirstName) || string.IsNullOrWhiteSpace(repLastName))
        {
            errors.Add("Authorised representative first name and last name are required.");
        }

        if (string.IsNullOrWhiteSpace(request.RepresentativeDesignation))
        {
            errors.Add("Authorised representative position / designation is required.");
        }

        if (string.IsNullOrWhiteSpace(request.RepresentativeEmail))
        {
            errors.Add("Authorised representative contact email is required.");
        }

        if (string.IsNullOrWhiteSpace(contactPhone))
        {
            errors.Add("Authorised representative contact phone number is required.");
        }

        // Document validation: Registration Certificate is required
        if (request.Documents == null || request.Documents.Count == 0)
        {
            errors.Add("Organisation Registration Certificate or NGO Registration Certificate is required.");
        }
        else
        {
            var hasRegCert = request.Documents.Any(d =>
                d != null &&
                (d.Category.Equals("RegistrationCertificate", StringComparison.OrdinalIgnoreCase) ||
                 d.Category.Contains("Certificate", StringComparison.OrdinalIgnoreCase)));

            if (!hasRegCert)
            {
                errors.Add("Organisation Registration Certificate or NGO Registration Certificate is required.");
            }
        }

        if (request.ConfirmAuthorisedRepresentative != true)
        {
            errors.Add("You must confirm that you are authorised to register this organisation.");
        }

        if (request.ConfirmAccurateInformation != true)
        {
            errors.Add("You must confirm that the information provided is accurate.");
        }

        if (request.AgreeTermsAndPrivacy != true)
        {
            errors.Add("You must agree to the CareLink Terms and Conditions and Privacy Policy.");
        }

        return errors;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<AuthResponseDto>.FailureResult("Email and password are required.");
        }

        var email = request.Email.Trim();
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == email.ToLower()) ||
                (u.UserName != null && u.UserName.ToLower() == email.ToLower()));

        user ??= await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);

        if (user == null)
        {
            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Repeated failed sign-in",
                User = email,
                Source = "Web Portal",
                Severity = "Medium",
                Description = $"Failed sign-in attempt: Unrecognised account '{email}'."
            });

            return ApiResponse<AuthResponseDto>.FailureResult("Invalid email or password.");
        }

        if (user.RequiresPasswordSetup ||
            string.Equals(user.AccountStatus, AccountStatuses.PendingPasswordSetup, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "Please complete your account setup using the password setup link sent to your email before logging in.",
                new List<string> { "PASSWORD_SETUP_REQUIRED" });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Repeated failed sign-in",
                User = user.Email ?? email,
                Source = "Web Portal",
                Severity = "High",
                Description = $"Failed sign-in attempt: Incorrect password submitted for '{user.Email}'."
            });

            return ApiResponse<AuthResponseDto>.FailureResult("Invalid email or password.");
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(user.AccountStatus, AccountStatuses.AdditionalInfoRequested, StringComparison.OrdinalIgnoreCase))
        {
            if (user.Organization != null &&
                (string.Equals(user.Organization.ApprovalStatus, AccountStatuses.AdditionalInfoRequested, StringComparison.OrdinalIgnoreCase) ||
                 !string.IsNullOrWhiteSpace(user.Organization.AdditionalInfoRequest)))
            {
                var infoNote = user.Organization.AdditionalInfoRequest ?? "Please provide the additional verification details requested by the administrator.";
                return ApiResponse<AuthResponseDto>.FailureResult(
                    $"Additional information is required to complete your organisation verification: {infoNote}",
                    new List<string> { "ACCOUNT_PENDING_APPROVAL", "ADDITIONAL_INFO_REQUIRED", infoNote });
            }

            return ApiResponse<AuthResponseDto>.FailureResult(
                "Your Partner Organization account is pending administrator approval. You cannot access protected features until an administrator approves your registration.",
                new List<string> { "ACCOUNT_PENDING_APPROVAL" });
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            var reasonDetail = !string.IsNullOrWhiteSpace(user.RejectionReason)
                ? $" Reason: {user.RejectionReason}"
                : string.Empty;
            return ApiResponse<AuthResponseDto>.FailureResult(
                $"Your Partner Organization registration was rejected by an administrator.{reasonDetail}",
                new List<string> { "ACCOUNT_REJECTED" });
        }

        if (!user.IsActive || string.Equals(user.AccountStatus, AccountStatuses.Disabled, StringComparison.OrdinalIgnoreCase))
        {
            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Unusual account activity",
                User = user.Email ?? email,
                Source = "Web Portal",
                Severity = "Medium",
                Description = $"Disabled user account '{user.Email}' attempted to authenticate."
            });

            return ApiResponse<AuthResponseDto>.FailureResult(
                "Your account has been disabled. Please contact an administrator.",
                new List<string> { "ACCOUNT_DISABLED" });
        }

        if (!user.EmailConfirmed)
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "Please verify your email before logging in. Check your inbox for the confirmation link.",
                new List<string> { "EMAIL_NOT_CONFIRMED" });
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<AuthResponseDto>.SuccessResult(
            responseDto, "Login successful.");
    }

    public async Task<ApiResponse<bool>> SubmitPartnerAdditionalInfoAsync(PartnerAdditionalInfoSubmissionDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<bool>.FailureResult("Login email and password are required.");
        }

        if (string.IsNullOrWhiteSpace(request.ResponseText))
        {
            return ApiResponse<bool>.FailureResult("Please provide the requested additional information.");
        }

        var email = request.Email.Trim();
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .ThenInclude(o => o!.Documents)
            .FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == email.ToLower()) ||
                (u.UserName != null && u.UserName.ToLower() == email.ToLower()));

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return ApiResponse<bool>.FailureResult("Invalid login email or password.");
        }

        if (user.Organization == null)
        {
            return ApiResponse<bool>.FailureResult("No Partner Organisation record is linked to this account.");
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase) && user.IsActive)
        {
            return ApiResponse<bool>.FailureResult("Your Partner Organisation account is already approved and active.");
        }

        var newDocs = new List<OrganizationDocument>();
        if (request.AdditionalDocuments != null && request.AdditionalDocuments.Count > 0)
        {
            foreach (var docDto in request.AdditionalDocuments)
            {
                var (isValidDoc, docError) = _documentStorageService.ValidateDocument(docDto);
                if (!isValidDoc)
                {
                    return ApiResponse<bool>.FailureResult(docError ?? "Invalid supporting document.");
                }

                var storedDoc = await _documentStorageService.StoreDocumentAsync(user.Organization.Id, docDto, email);
                user.Organization.Documents.Add(storedDoc);
                newDocs.Add(storedDoc);
            }
        }

        user.Organization.AdditionalInfoResponse = request.ResponseText.Trim();
        user.Organization.AdditionalInfoSubmittedAt = DateTime.UtcNow;
        user.Organization.ApprovalStatus = AccountStatuses.PendingApproval;
        user.Organization.UpdatedAt = DateTime.UtcNow;
        user.AccountStatus = AccountStatuses.PendingApproval;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        if (_verificationService != null)
        {
            try
            {
                await _verificationService.RegisterPartnerOrganizationRequestAsync(user.Organization, user);
            }
            catch
            {
                // Non-fatal
            }
        }

        if (_auditLogService != null)
        {
            try
            {
                await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
                {
                    Action = "Partner organisation submitted additional information",
                    Actor = $"{user.FullName} ({user.Email})",
                    Record = user.Organization.Name,
                    Change = "AdditionalInfoRequested → PendingApproval",
                    Rationale = $"{request.ResponseText.Trim()} (Attached {newDocs.Count} new document(s))",
                    Type = "Organisation",
                    Environment = "Production"
                });
            }
            catch
            {
                // Non-fatal
            }
        }

        return ApiResponse<bool>.SuccessResult(
            true,
            "Your additional verification information has been submitted and your organisation is back in the administrator review queue.");
    }

    public async Task<ApiResponse<bool>> ValidateSetupTokenAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return ApiResponse<bool>.FailureResult(
                "Email address and setup token are required.",
                new List<string> { "INVALID_TOKEN" });
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult(
                "Invalid setup link or account does not exist.",
                new List<string> { "INVALID_TOKEN" });
        }

        if (user.PasswordSetupTokenUsed)
        {
            return ApiResponse<bool>.FailureResult(
                "This account setup link has already been used.",
                new List<string> { "TOKEN_ALREADY_USED" });
        }

        if (user.PasswordSetupTokenExpiresAt.HasValue && user.PasswordSetupTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return ApiResponse<bool>.FailureResult(
                "This account setup link has expired. Please contact an administrator to resend the invitation.",
                new List<string> { "EXPIRED_TOKEN" });
        }

        var rawToken = UrlTokenEncoder.SafeDecode(token);
        var isValid = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
            rawToken);

        if (!isValid)
        {
            return ApiResponse<bool>.FailureResult(
                "Invalid or expired account setup token.",
                new List<string> { "INVALID_TOKEN" });
        }

        return ApiResponse<bool>.SuccessResult(true, "Account setup token is valid.");
    }

    public async Task<ApiResponse<bool>> SetupPasswordAsync(SetupPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
        {
            return ApiResponse<bool>.FailureResult(
                "Email address and setup token are required.",
                new List<string> { "INVALID_TOKEN" });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return ApiResponse<bool>.FailureResult("New password must be at least 6 characters.");
        }

        if (!string.IsNullOrEmpty(request.ConfirmPassword) && request.NewPassword != request.ConfirmPassword)
        {
            return ApiResponse<bool>.FailureResult("Passwords do not match.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult(
                "Invalid setup request or user does not exist.",
                new List<string> { "INVALID_TOKEN" });
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Disabled, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(user.AccountStatus, AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<bool>.FailureResult(
                "This account has been disabled or restricted by an administrator.",
                new List<string> { "ACCOUNT_DISABLED" });
        }

        if (user.PasswordSetupTokenUsed)
        {
            return ApiResponse<bool>.FailureResult(
                "This account setup link has already been used and cannot be reused.",
                new List<string> { "TOKEN_ALREADY_USED" });
        }

        if (user.PasswordSetupTokenExpiresAt.HasValue && user.PasswordSetupTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return ApiResponse<bool>.FailureResult(
                "This account setup link has expired. Please request a new setup link from your administrator.",
                new List<string> { "EXPIRED_TOKEN" });
        }

        var rawToken = UrlTokenEncoder.SafeDecode(request.Token);
        var resetResult = await _userManager.ResetPasswordAsync(user, rawToken, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = resetResult.Errors.Select(e => e.Description).ToList();
            errors.Add("INVALID_TOKEN");
            return ApiResponse<bool>.FailureResult(
                "Account setup failed. The setup link is invalid, expired, or has already been used.",
                errors);
        }

        user.RequiresPasswordSetup = false;
        user.PasswordSetupTokenUsed = true;
        user.EmailConfirmed = true;

        if (!string.Equals(user.AccountStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase))
        {
            user.IsActive = true;
            user.AccountStatus = AccountStatuses.Active;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.UpdateAsync(user);
        _permissionService.InvalidateCache(userId: user.Id);

        _logger.LogInformation("Account setup and password configuration completed for user: {Email}", user.Email);

        return ApiResponse<bool>.SuccessResult(
            true,
            "Your password has been set and your account is now active. You may now sign in.");
    }

    public async Task<ApiResponse<bool>> VerifyEmailAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return ApiResponse<bool>.FailureResult("Email address and verification token are required.");
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("Invalid verification request or user does not exist.");
        }

        if (user.EmailConfirmed)
        {
            return ApiResponse<bool>.SuccessResult(true, "Your email is already verified. You can log in to your account.");
        }

        var rawToken = UrlTokenEncoder.SafeDecode(token);
        var confirmResult = await _userManager.ConfirmEmailAsync(user, rawToken);
        if (!confirmResult.Succeeded)
        {
            var errors = confirmResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.FailureResult(
                "Email verification failed. The confirmation link is invalid, expired, or has already been used.",
                errors);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return ApiResponse<bool>.SuccessResult(
            true, "Email has been successfully verified! You may now sign in to CareLink.");
    }

    public async Task<ApiResponse<bool>> ResendVerificationEmailAsync(string email)
    {
        const string genericResponse =
            "If your email address is registered and unconfirmed, a verification link has been sent to your inbox.";

        if (string.IsNullOrWhiteSpace(email))
        {
            return ApiResponse<bool>.FailureResult("Email address is required.");
        }

        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToUpperInvariant();

        var cooldownKey = $"resend_cooldown_{normalizedEmail}";
        if (_memoryCache.TryGetValue(cooldownKey, out DateTime nextAllowedUtc))
        {
            var remainingSeconds = Math.Max(1, (int)(nextAllowedUtc - DateTime.UtcNow).TotalSeconds);
            return ApiResponse<bool>.FailureResult(
                $"A verification request was recently submitted. Please wait {remainingSeconds} seconds before requesting another.");
        }

        _memoryCache.Set(cooldownKey, DateTime.UtcNow.AddSeconds(60), TimeSpan.FromMinutes(2));

        var user = await _userManager.FindByEmailAsync(trimmedEmail);
        if (user == null || user.EmailConfirmed)
        {
            return ApiResponse<bool>.SuccessResult(true, genericResponse);
        }

        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = UrlTokenEncoder.Encode(rawToken);

        var baseUrl = !string.IsNullOrWhiteSpace(_emailSettings.VerificationBaseUrl)
            ? _emailSettings.VerificationBaseUrl
            : "http://localhost:5277/api/auth/verify-email";

        var confirmationUrl = $"{baseUrl}?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

        await _emailService.SendVerificationEmailAsync(user.Email!, user.FullName, confirmationUrl);

        return ApiResponse<bool>.SuccessResult(true, genericResponse);
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserDto>.FailureResult("User not found.");
        }

        if (!user.IsActive ||
            user.RequiresPasswordSetup ||
            !string.Equals(user.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<UserDto>.FailureResult("Your account is not active.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        return ApiResponse<UserDto>.SuccessResult(MapToUserDto(user, roles, permissions));
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles, IList<string>? permissions = null)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? user.Email ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            AlternativePhoneNumber = user.AlternativePhoneNumber,
            Designation = user.Designation,
            NIC = user.NIC,
            Address = user.Address,
            Roles = roles,
            Permissions = permissions ?? new List<string>(),
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            OrganizationApprovalStatus = user.Organization?.ApprovalStatus,
            AdditionalInfoRequest = user.Organization?.AdditionalInfoRequest,
            IsActive = user.IsActive,
            AccountStatus = string.IsNullOrWhiteSpace(user.AccountStatus)
                ? (user.IsActive ? AccountStatuses.Active : AccountStatuses.Disabled)
                : user.AccountStatus,
            RejectionReason = user.RejectionReason,
            Skills = user.Skills,
            Availability = user.Availability,
            PreferredLocation = user.PreferredLocation,
            RequiresPasswordSetup = user.RequiresPasswordSetup,
            CreatedAt = user.CreatedAt
        };
    }
}
