using System.Security.Claims;
using System.Text.Json;
using CareLink.API.Authorization;
using CareLink.API.Controllers;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Services;
using CareLink.Tests.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CareLink.Tests.UnitTests.Services;

public class UserManagementAndAuthTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<ISecurityEventService> _securityEventMock;
    private readonly IPermissionService _permissionService;
    private readonly AuthService _authService;
    private readonly UserService _userService;

    public UserManagementAndAuthTests()
    {
        var (context, connection) = TestDbContextFactory.CreateSqliteInMemory();
        _dbContext = context;
        _connection = connection;

        _emailMock = new Mock<IEmailService>();
        _emailMock.Setup(e => e.SendCitizenWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _emailMock.Setup(e => e.SendVolunteerWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(true);
        _emailMock.Setup(e => e.SendPartnerPendingApprovalEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _emailMock.Setup(e => e.SendPartnerApprovalEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _emailMock.Setup(e => e.SendPartnerRejectionEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(true);
        _emailMock.Setup(e => e.SendAdminCreatedUserEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _securityEventMock = new Mock<ISecurityEventService>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddMemoryCache();
        services.AddSingleton(_dbContext);

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        var inMemoryConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "CareLinkTestingSuperSecretKeyForAutomatedTests2026!@#$%",
                ["Jwt:Issuer"] = "CareLinkAPI",
                ["Jwt:Audience"] = "CareLinkClient",
                ["Jwt:ExpiryHours"] = "24"
            })
            .Build();

        var emailOptions = Options.Create(new EmailSettings
        {
            SenderName = "CareLink Admin",
            SenderEmail = "carelink.platform.admin@gmail.com",
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587,
            Username = "carelink.platform.admin@gmail.com",
            EnableSsl = true,
            FrontendBaseUrl = "http://localhost:5173",
            PasswordSetupBaseUrl = "http://localhost:5173/setup-account",
            VerificationBaseUrl = "http://localhost:5277/api/auth/verify-email"
        });

        _serviceProvider = services.BuildServiceProvider();
        _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var memoryCache = _serviceProvider.GetRequiredService<IMemoryCache>();

        _permissionService = new PermissionService(
            _dbContext,
            _userManager,
            _roleManager,
            memoryCache,
            NullLogger<PermissionService>.Instance);

        var jwtGenerator = new JwtTokenGenerator(inMemoryConfig);

        _authService = new AuthService(
            _userManager,
            _roleManager,
            jwtGenerator,
            _emailMock.Object,
            emailOptions,
            memoryCache,
            NullLogger<AuthService>.Instance,
            _securityEventMock.Object,
            _permissionService);

        _userService = new UserService(
            _userManager,
            _roleManager,
            _permissionService,
            _emailMock.Object,
            emailOptions,
            NullLogger<UserService>.Instance);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _dbContext.Dispose();
        _connection.Dispose();
    }

    // =========================================================================
    // 1. CITIZEN REGISTRATION & LOGIN TESTS
    // =========================================================================

    [Fact]
    public async Task CitizenRegistration_Succeeds_ActiveImmediately_CanLoginWithoutAdminApproval()
    {
        var request = new RegisterRequestDto
        {
            FullName = "Nimal Perera",
            Email = "nimal.citizen@carelink.org",
            Password = "Password123!",
            PhoneNumber = "+94771112233",
            Address = "Colombo 05",
            Role = "Citizen"
        };

        var regResult = await _authService.RegisterAsync(request);

        Assert.True(regResult.Success);
        Assert.NotNull(regResult.Data);
        Assert.Equal(AccountStatuses.Active, regResult.Data.AccountStatus);
        Assert.True(regResult.Data.IsActive);
        Assert.False(regResult.Data.RequiresAdminApproval);
        Assert.False(string.IsNullOrWhiteSpace(regResult.Data.Token));
        Assert.Contains(UserRoles.Citizen, regResult.Data.User!.Roles);
        Assert.Equal("nimal.citizen@carelink.org", regResult.Data.User.UserName);

        _emailMock.Verify(
            e => e.SendCitizenWelcomeEmailAsync("nimal.citizen@carelink.org", "Nimal Perera"),
            Times.Once);

        var loginResult = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "nimal.citizen@carelink.org",
            Password = "Password123!"
        });

        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.Data?.Token);
        Assert.Contains(UserRoles.Citizen, loginResult.Data!.User.Roles);
    }

    // =========================================================================
    // 2. VOLUNTEER REGISTRATION & LOGIN TESTS
    // =========================================================================

    [Fact]
    public async Task VolunteerRegistration_Succeeds_ActiveImmediately_CreatesProfile_AndCanLogin()
    {
        var request = new RegisterRequestDto
        {
            FullName = "Kavindi Silva",
            Email = "kavindi.volunteer@carelink.org",
            Password = "VolunteerPass123!",
            PhoneNumber = "+94772223344",
            Role = "Volunteer",
            Skills = "First Aid, Swift Water Rescue, Medical Triage",
            Availability = "Immediate",
            PreferredLocation = "North Ward"
        };

        var regResult = await _authService.RegisterAsync(request);

        Assert.True(regResult.Success);
        Assert.NotNull(regResult.Data);
        Assert.Equal(AccountStatuses.Active, regResult.Data.AccountStatus);
        Assert.True(regResult.Data.IsActive);
        Assert.False(regResult.Data.RequiresAdminApproval);
        Assert.False(string.IsNullOrWhiteSpace(regResult.Data.Token));
        Assert.Contains(UserRoles.Volunteer, regResult.Data.User!.Roles);
        Assert.Equal("First Aid, Swift Water Rescue, Medical Triage", regResult.Data.User.Skills);
        Assert.Equal("Immediate", regResult.Data.User.Availability);
        Assert.Equal("North Ward", regResult.Data.User.PreferredLocation);

        _emailMock.Verify(
            e => e.SendVolunteerWelcomeEmailAsync(
                "kavindi.volunteer@carelink.org",
                "Kavindi Silva",
                "First Aid, Swift Water Rescue, Medical Triage",
                "Immediate"),
            Times.Once);

        var loginResult = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "kavindi.volunteer@carelink.org",
            Password = "VolunteerPass123!"
        });

        Assert.True(loginResult.Success);
        Assert.Contains(UserRoles.Volunteer, loginResult.Data!.User.Roles);
    }

    // =========================================================================
    // 3. PARTNER ORGANIZATION REGISTRATION, APPROVAL & REJECTION TESTS
    // =========================================================================

    [Fact]
    public async Task PartnerRegistration_SetsPendingApproval_BlocksLoginUntilAdminApproves_ThenAllowsLogin()
    {
        var regRequest = new RegisterRequestDto
        {
            FullName = "Sara Khan",
            Email = "sara@globalrelief.org",
            Password = "PartnerPass123!",
            PhoneNumber = "+94112223344",
            Role = "PartnerOfficer",
            OrganizationName = "Global Relief Network",
            OrganizationType = "International NGO",
            RegistrationNumber = "NGO-2026-881"
        };

        var regResult = await _authService.RegisterAsync(regRequest);

        Assert.True(regResult.Success);
        Assert.NotNull(regResult.Data);
        Assert.Equal(AccountStatuses.PendingApproval, regResult.Data.AccountStatus);
        Assert.False(regResult.Data.IsActive);
        Assert.True(regResult.Data.RequiresAdminApproval);
        Assert.Null(regResult.Data.Token);
        Assert.Contains(UserRoles.PartnerOfficer, regResult.Data.User!.Roles);
        Assert.Equal("Global Relief Network", regResult.Data.User.OrganizationName);

        _emailMock.Verify(
            e => e.SendPartnerPendingApprovalEmailAsync("sara@globalrelief.org", "Sara Khan", "Global Relief Network"),
            Times.Once);

        var preApprovalLogin = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "sara@globalrelief.org",
            Password = "PartnerPass123!"
        });

        Assert.False(preApprovalLogin.Success);
        Assert.Contains("ACCOUNT_PENDING_APPROVAL", preApprovalLogin.Errors!);

        var prePerms = await _permissionService.GetUserEffectivePermissionsAsync(regResult.Data.Id);
        Assert.Empty(prePerms);

        var approveResult = await _userService.ApprovePartnerAsync(
            regResult.Data.Id,
            "Verified NGO registration certificate.",
            "System Administrator");

        Assert.True(approveResult.Success);
        Assert.Equal(AccountStatuses.Active, approveResult.Data!.AccountStatus);
        Assert.True(approveResult.Data.IsActive);

        _emailMock.Verify(
            e => e.SendPartnerApprovalEmailAsync(
                "sara@globalrelief.org",
                "Sara Khan",
                "Global Relief Network",
                It.IsAny<string>()),
            Times.Once);

        var postApprovalLogin = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "sara@globalrelief.org",
            Password = "PartnerPass123!"
        });

        Assert.True(postApprovalLogin.Success);
        Assert.NotNull(postApprovalLogin.Data?.Token);
        Assert.Contains(UserRoles.PartnerOfficer, postApprovalLogin.Data!.User.Roles);
        Assert.NotEmpty(postApprovalLogin.Data.User.Permissions);
    }

    [Fact]
    public async Task PartnerRejection_SetsRejectedStatus_StoresReason_SendsEmail_AndBlocksLogin()
    {
        var regResult = await _authService.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Unverified Partner",
            Email = "apply@unverified-ngo.org",
            Password = "PartnerPass123!",
            PhoneNumber = "+94119998877",
            Role = "Partner Organization",
            OrganizationName = "Unverified Relief Org",
            OrganizationType = "Local NGO"
        });

        Assert.True(regResult.Success);
        var partnerId = regResult.Data!.Id;

        var rejectResult = await _userService.RejectPartnerAsync(
            partnerId,
            "Invalid NGO registration number.",
            "System Administrator");

        Assert.True(rejectResult.Success);
        Assert.Equal(AccountStatuses.Rejected, rejectResult.Data!.AccountStatus);
        Assert.False(rejectResult.Data.IsActive);
        Assert.Equal("Invalid NGO registration number.", rejectResult.Data.RejectionReason);

        _emailMock.Verify(
            e => e.SendPartnerRejectionEmailAsync(
                "apply@unverified-ngo.org",
                "Unverified Partner",
                "Unverified Relief Org",
                "Invalid NGO registration number."),
            Times.Once);

        var loginAttempt = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "apply@unverified-ngo.org",
            Password = "PartnerPass123!"
        });

        Assert.False(loginAttempt.Success);
        Assert.Contains("ACCOUNT_REJECTED", loginAttempt.Errors!);
        Assert.Contains("Invalid NGO registration number.", loginAttempt.Message);
    }

    // =========================================================================
    // 4. PUBLIC REGISTRATION ROLE ESCALATION PREVENTION TESTS
    // =========================================================================

    [Theory]
    [InlineData("Admin")]
    [InlineData("SystemAdmin")]
    [InlineData("EmergencyCoordinator")]
    [InlineData("Coordinator")]
    [InlineData("ResourceManager")]
    [InlineData("LogisticsManager")]
    [InlineData("ShelterAdmin")]
    [InlineData("IncidentManager")]
    [InlineData("VolunteerCoordinator")]
    [InlineData("Donor")]
    public async Task PublicRegistration_BlocksAllInternalAndAdminRoles(string forbiddenRole)
    {
        var result = await _authService.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Attacker User",
            Email = $"attacker.{forbiddenRole.ToLower()}@example.com",
            Password = "Password123!",
            PhoneNumber = "+94770001111",
            Role = forbiddenRole
        });

        Assert.False(result.Success);
        Assert.Contains("Only Citizen, Volunteer, and Partner Organization", result.Message);

        var dbUser = await _userManager.FindByEmailAsync($"attacker.{forbiddenRole.ToLower()}@example.com");
        Assert.Null(dbUser);
    }

    // =========================================================================
    // 5. ADMIN-CREATED USER, NO TOKEN IN API RESPONSE & SINGLE-USE SETUP TESTS
    // =========================================================================

    [Theory]
    [InlineData("Coordinator", UserRoles.EmergencyCoordinator)]
    [InlineData("Resource Manager", UserRoles.ResourceManager)]
    [InlineData("Shelter Admin", UserRoles.ShelterAdministrator)]
    [InlineData("Admin", UserRoles.Admin)]
    [InlineData("System Administrator", UserRoles.SystemAdmin)]
    [InlineData("Case Manager", UserRoles.CaseManager)]
    [InlineData("Funding Officer", UserRoles.FundingOfficer)]
    [InlineData("Partner Officer", UserRoles.PartnerOfficer)]
    [InlineData("Volunteer Coordinator", UserRoles.VolunteerCoordinator)]
    public async Task AdminCreateUser_NeverExposesTokenInApiResponse_SendsEmail_AndCompletesSingleUsePasswordSetup(
        string requestedRoleInput,
        string expectedNormalizedRole)
    {
        var email = $"staff.{expectedNormalizedRole.ToLower()}@carelink.org";
        string? capturedSetupUrl = null;

        _emailMock
            .Setup(e => e.SendAdminCreatedUserEmailAsync(email, "Internal Officer", It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string>((_, _, _, url) => capturedSetupUrl = url)
            .ReturnsAsync(true);

        var createResult = await _userService.CreateUserAsync(new CreateUserRequestDto
        {
            FullName = "Internal Officer",
            Email = email,
            PhoneNumber = "+94778889900",
            Role = requestedRoleInput,
            OrganizationName = "National Disaster Management Centre"
        });

        Assert.True(createResult.Success);
        Assert.NotNull(createResult.Data);
        Assert.Equal(email, createResult.Data.Email);
        Assert.Equal(email, createResult.Data.UserName);
        Assert.Contains(expectedNormalizedRole, createResult.Data.Roles);
        Assert.Equal(AccountStatuses.PendingPasswordSetup, createResult.Data.AccountStatus);
        Assert.True(createResult.Data.RequiresPasswordSetup);
        Assert.True(createResult.Data.EmailSent);
        Assert.Equal("Sent", createResult.Data.EmailDeliveryStatus);

        // CRITICAL SECURITY ASSERTION: API response JSON never contains setup URL or setup token
        var serializedApiResponse = JsonSerializer.Serialize(createResult);
        Assert.DoesNotContain("/setup-account", serializedApiResponse);
        Assert.DoesNotContain("/setup-password", serializedApiResponse);
        Assert.DoesNotContain("token=", serializedApiResponse);

        // Email service received the setup URL
        Assert.NotNull(capturedSetupUrl);
        Assert.Contains("/setup-account?email=", capturedSetupUrl);
        Assert.Contains("token=", capturedSetupUrl);

        var uri = new Uri(capturedSetupUrl!);
        var queryParts = uri.Query.TrimStart('?').Split('&')
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
        var token = queryParts["token"];

        // User cannot log in before completing password setup
        var prematureLogin = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = email,
            Password = "AnyPassword123!"
        });
        Assert.False(prematureLogin.Success);
        Assert.Contains("PASSWORD_SETUP_REQUIRED", prematureLogin.Errors!);

        // Validate token endpoint
        var validateToken = await _authService.ValidateSetupTokenAsync(email, token);
        Assert.True(validateToken.Success);

        // Complete password setup
        var setupResult = await _authService.SetupPasswordAsync(new SetupPasswordRequestDto
        {
            Email = email,
            Token = token,
            NewPassword = "MySecureNewPassword123!",
            ConfirmPassword = "MySecureNewPassword123!"
        });
        Assert.True(setupResult.Success);

        // Verify token is single-use and cannot be reused
        var reuseResult = await _authService.SetupPasswordAsync(new SetupPasswordRequestDto
        {
            Email = email,
            Token = token,
            NewPassword = "SecondAttemptPassword123!",
            ConfirmPassword = "SecondAttemptPassword123!"
        });
        Assert.False(reuseResult.Success);
        Assert.Contains("TOKEN_ALREADY_USED", reuseResult.Errors!);

        // User can now log in with their newly configured password
        var loginResult = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = email,
            Password = "MySecureNewPassword123!"
        });
        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.Data?.Token);
        Assert.Equal(AccountStatuses.Active, loginResult.Data!.User.AccountStatus);
        Assert.Contains(expectedNormalizedRole, loginResult.Data.User.Roles);
    }

    [Fact]
    public async Task SetupPassword_RejectsExpiredToken_AndInvalidToken()
    {
        var createResult = await _userService.CreateUserAsync(new CreateUserRequestDto
        {
            FullName = "Expired Token User",
            Email = "expired.staff@carelink.org",
            Role = "EmergencyCoordinator"
        });
        Assert.True(createResult.Success);

        var dbUser = await _userManager.FindByEmailAsync("expired.staff@carelink.org");
        Assert.NotNull(dbUser);

        // Test invalid token first
        var invalidResult = await _authService.SetupPasswordAsync(new SetupPasswordRequestDto
        {
            Email = "expired.staff@carelink.org",
            Token = "completely-invalid-tampered-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        });
        Assert.False(invalidResult.Success);
        Assert.Contains("INVALID_TOKEN", invalidResult.Errors!);

        // Expire the setup token
        dbUser!.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        await _userManager.UpdateAsync(dbUser);

        var rawToken = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
        var encodedToken = UrlTokenEncoder.Encode(rawToken);

        var setupResult = await _authService.SetupPasswordAsync(new SetupPasswordRequestDto
        {
            Email = "expired.staff@carelink.org",
            Token = encodedToken,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        });

        Assert.False(setupResult.Success);
        Assert.Contains("EXPIRED_TOKEN", setupResult.Errors!);
    }

    [Fact]
    public async Task SmtpFailure_OnCreateUser_PreservesAccount_ReportsEmailFailed_AndResendInvitationInvalidatesOldToken()
    {
        string? firstTokenUrl = null;
        string? secondTokenUrl = null;

        // First call: simulate SMTP failure (returns false)
        _emailMock
            .Setup(e => e.SendAdminCreatedUserEmailAsync("oshadi@carelink.org", "Oshadi Vidarshana", "Emergency Coordinator", It.IsAny<string>()))
            .Callback<string, string, string, string>((_, _, _, url) => firstTokenUrl = url)
            .ReturnsAsync(false);

        var createResult = await _userService.CreateUserAsync(new CreateUserRequestDto
        {
            FullName = "Oshadi Vidarshana",
            Email = "oshadi@carelink.org",
            Role = "Emergency Coordinator"
        });

        // Account is preserved in DB, but EmailSent is honestly reported as false
        Assert.True(createResult.Success);
        Assert.NotNull(createResult.Data);
        Assert.False(createResult.Data.EmailSent);
        Assert.Equal("Failed", createResult.Data.EmailDeliveryStatus);
        Assert.Contains("could not be sent", createResult.Message, StringComparison.OrdinalIgnoreCase);

        var firstToken = ExtractTokenFromUrl(firstTokenUrl!);

        // Second call: Resend Invitation succeeds and invalidates first token
        _emailMock
            .Setup(e => e.SendAdminCreatedUserEmailAsync("oshadi@carelink.org", "Oshadi Vidarshana", "Emergency Coordinator", It.IsAny<string>()))
            .Callback<string, string, string, string>((_, _, _, url) => secondTokenUrl = url)
            .ReturnsAsync(true);

        var resendResult = await _userService.ResendInvitationAsync(createResult.Data.Id);
        Assert.True(resendResult.Success);
        Assert.True(resendResult.Data!.EmailSent);
        Assert.Equal("Sent", resendResult.Data.EmailDeliveryStatus);

        var secondToken = ExtractTokenFromUrl(secondTokenUrl!);

        // Old token is now invalid because security stamp was rotated
        var oldTokenValidation = await _authService.ValidateSetupTokenAsync("oshadi@carelink.org", firstToken);
        Assert.False(oldTokenValidation.Success);

        // Fresh token works
        var newTokenValidation = await _authService.ValidateSetupTokenAsync("oshadi@carelink.org", secondToken);
        Assert.True(newTokenValidation.Success);
    }

    private static string ExtractTokenFromUrl(string url)
    {
        var uri = new Uri(url);
        var queryParts = uri.Query.TrimStart('?').Split('&')
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
        return queryParts["token"];
    }

    // =========================================================================
    // 6. EMAIL TEMPLATES & FAILURE HANDLING TESTS
    // =========================================================================

    [Fact]
    public void EmailTemplates_ContainExpectedDetails_AndNeverExposePlainTextPasswords()
    {
        var setupUrl = "http://localhost:5173/setup-account?email=coord%40carelink.org&token=xyz123";
        var subject = EmailTemplates.GetAdminCreatedUserSubject();
        var adminHtml = EmailTemplates.GetAdminCreatedUserHtml(
            "Rohan Fernando",
            "coord@carelink.org",
            "Emergency Coordinator",
            setupUrl);
        var adminText = EmailTemplates.GetAdminCreatedUserPlainText(
            "Rohan Fernando",
            "coord@carelink.org",
            "Emergency Coordinator",
            setupUrl);

        Assert.Equal("Your CareLink Account Has Been Created", subject);
        Assert.Contains("Rohan Fernando", adminHtml);
        Assert.Contains("Emergency Coordinator", adminHtml);
        Assert.Contains("coord@carelink.org", adminHtml);
        Assert.Contains("Set Up My CareLink Account", adminHtml);
        Assert.Contains(setupUrl, adminText);
        Assert.DoesNotContain("CareLink@2026", adminHtml);
        Assert.DoesNotContain("CareLink@2026", adminText);

        var partnerRejectHtml = EmailTemplates.GetPartnerRejectedHtml(
            "Sara Khan",
            "sara@ngo.org",
            "Relief NGO",
            "Missing tax compliance certificate.");
        Assert.Contains("Missing tax compliance certificate.", partnerRejectHtml);
    }

    [Fact]
    public async Task EmailService_ReturnsFalse_WhenSmtpPasswordIsMissing()
    {
        var settingsWithoutPassword = Options.Create(new EmailSettings
        {
            SenderName = "CareLink Admin",
            SenderEmail = "carelink.platform.admin@gmail.com",
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587,
            Username = "carelink.platform.admin@gmail.com",
            Password = "", // missing app password
            EnableSsl = true
        });

        var realEmailService = new EmailService(settingsWithoutPassword, NullLogger<EmailService>.Instance, allowAmbientSecretFallback: false);
        var result = await realEmailService.SendAdminCreatedUserEmailWithResultAsync(
            "recipient@example.com",
            "Test Recipient",
            "Emergency Coordinator",
            "http://localhost:5173/setup-account?email=recipient%40example.com&token=abc");

        Assert.False(result.Success);
        Assert.Equal(EmailFailureCategories.ConfigurationMissing, result.FailureCategory);
        Assert.Contains("EmailSettings:Password", result.DiagnosticMessage);

        var diag = realEmailService.GetDiagnostics();
        Assert.False(diag.IsPasswordConfigured);
        Assert.Equal("smtp.gmail.com", diag.SmtpServer);
        Assert.Equal(587, diag.SmtpPort);
        Assert.True(diag.SenderMatchesUsername);
    }

    [Fact]
    public void EmailService_GetDiagnostics_NormalizesGrouped16CharAppPassword()
    {
        var settingsWithSpacedAppPassword = Options.Create(new EmailSettings
        {
            SenderName = "CareLink Admin",
            SenderEmail = "carelink.platform.admin@gmail.com",
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587,
            Username = "carelink.platform.admin@gmail.com",
            Password = " abcd efgh ijkl mnop ",
            EnableSsl = true
        });

        var emailService = new EmailService(settingsWithSpacedAppPassword, NullLogger<EmailService>.Instance);
        var diag = emailService.GetDiagnostics();

        Assert.True(diag.IsPasswordConfigured);
        Assert.Equal(16, diag.ConfiguredPasswordLength);
        Assert.True(diag.IsAppPasswordFormatValid);
        Assert.True(diag.SenderMatchesUsername);
    }

    // =========================================================================
    // 7. DISABLED USER, DUPLICATE EMAIL & AUTHORIZATION TESTS
    // =========================================================================

    [Fact]
    public async Task DisabledUser_CannotLogin_AndCannotPassAuthorizationHandler()
    {
        var regResult = await _authService.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Active Citizen",
            Email = "active.to.disable@carelink.org",
            Password = "Password123!",
            PhoneNumber = "+94775554433",
            Role = "Citizen"
        });
        Assert.True(regResult.Success);
        var userId = regResult.Data!.Id;

        var disableResult = await _userService.UpdateUserStatusAsync(userId, false);
        Assert.True(disableResult.Success);

        var loginAttempt = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "active.to.disable@carelink.org",
            Password = "Password123!"
        });
        Assert.False(loginAttempt.Success);
        Assert.Contains("ACCOUNT_DISABLED", loginAttempt.Errors!);

        var handler = new PermissionAuthorizationHandler(_serviceProvider);
        var requirement = new PermissionRequirement(Permissions.Requests.Create);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, UserRoles.Citizen)
        ], "TestAuth"));

        var authContext = new AuthorizationHandlerContext([requirement], principal, null);
        await handler.HandleAsync(authContext);

        Assert.False(authContext.HasSucceeded);
    }

    [Fact]
    public async Task DuplicateEmail_IsRejectedInBothPublicRegistrationAndAdminCreation()
    {
        await _authService.RegisterAsync(new RegisterRequestDto
        {
            FullName = "First User",
            Email = "duplicate@carelink.org",
            Password = "Password123!",
            PhoneNumber = "+94771110000",
            Role = "Citizen"
        });

        var secondPublic = await _authService.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Second User",
            Email = "duplicate@carelink.org",
            Password = "Password123!",
            PhoneNumber = "+94772220000",
            Role = "Volunteer"
        });
        Assert.False(secondPublic.Success);
        Assert.Contains("already exists", secondPublic.Message, StringComparison.OrdinalIgnoreCase);

        var secondAdmin = await _userService.CreateUserAsync(new CreateUserRequestDto
        {
            FullName = "Admin Dup",
            Email = "duplicate@carelink.org",
            Role = "EmergencyCoordinator"
        });
        Assert.False(secondAdmin.Success);
        Assert.Contains("already exists", secondAdmin.Message, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // 8. FULL 3-STEP PARTNER ORGANISATION REGISTRATION, DOCUMENT STORAGE & VERIFICATION WORKFLOW
    // =========================================================================

    [Fact]
    public async Task PartnerOrganizationFullRegistration_SavesAllSections_ValidatesDocuments_AndCompletesVerificationWorkflow()
    {
        _emailMock.Setup(e => e.SendPartnerAdditionalInfoEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(true);
        _emailMock.Setup(e => e.SendAdminNewPartnerNotificationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(true);

        var pdfBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.7 Certified NGO Registration Certificate Content");
        var pdfBase64 = Convert.ToBase64String(pdfBytes);

        var fullRequest = new RegisterRequestDto
        {
            Role = "PartnerOfficer",
            FullName = "Dr. Chamari Fernando",
            RepresentativeFirstName = "Chamari",
            RepresentativeLastName = "Fernando",
            RepresentativeDesignation = "National Humanitarian Director",
            RepresentativeEmail = "chamari.f@lankarelief.org",
            RepresentativePhone = "+94771239876",
            RepresentativeAlternativePhone = "+94112349876",
            Email = "login.chamari@lankarelief.org",
            Password = "SecurePartnerPassword123!",
            ConfirmPassword = "SecurePartnerPassword123!",
            PhoneNumber = "+94771239876",
            OrganizationName = "Lanka Humanitarian Relief Foundation",
            OrganizationType = "Other",
            OtherOrganizationType = "Regional Disaster Medical Consortium",
            RegistrationCountry = "Sri Lanka",
            RegistrationNumber = "NGO-SL-2026-990",
            YearEstablished = 2016,
            Website = "https://lankarelief.org",
            OrganizationEmail = "hq@lankarelief.org",
            OrganizationPhone = "+94112228899",
            SecondaryContactPhone = "+94112228800",
            OrganizationAddress = "88 Baudhaloka Mawatha, Colombo 04",
            District = "Colombo",
            Province = "Western Province",
            OrganizationDescription = "Island-wide disaster medical triage, mobile field hospitals, and clean water distribution.",
            AreasOfOperation = ["Colombo", "Gampaha", "Kalutara", "Ratnapura"],
            AreasOfAssistance = ["Emergency Medical Assistance", "Water and Sanitation", "Other"],
            OtherAreaOfAssistance = "Psychosocial Trauma Counselling",
            OperationalCapacity = "4 mobile medical units, 60 certified paramedics, and 2 water purification trucks.",
            EmergencyResponseAvailability = "Available for emergency response",
            AdditionalResources = "4x4 ambulances, portable generators, cold-chain vaccine storage",
            ConfirmAuthorisedRepresentative = true,
            ConfirmAccurateInformation = true,
            AgreeTermsAndPrivacy = true,
            Documents =
            [
                new RegisterDocumentUploadDto
                {
                    Category = "RegistrationCertificate",
                    FileName = "Lanka_Relief_NGO_Certificate.pdf",
                    ContentType = "application/pdf",
                    FileSizeBytes = pdfBytes.Length,
                    Base64Content = pdfBase64
                },
                new RegisterDocumentUploadDto
                {
                    Category = "AuthorisationLetter",
                    FileName = "Board_Authorisation_Chamari.pdf",
                    ContentType = "application/pdf",
                    FileSizeBytes = pdfBytes.Length,
                    Base64Content = pdfBase64
                }
            ]
        };

        var regResult = await _authService.RegisterAsync(fullRequest);

        Assert.True(regResult.Success);
        Assert.NotNull(regResult.Data);
        Assert.Equal(AccountStatuses.PendingApproval, regResult.Data.AccountStatus);
        Assert.False(regResult.Data.IsActive);
        Assert.True(regResult.Data.RequiresAdminApproval);
        Assert.Equal(2, regResult.Data.UploadedDocumentsCount);
        Assert.NotNull(regResult.Data.OrganizationId);

        // Verify Organization & Documents persisted in database
        var savedOrg = await _dbContext.Organizations
            .Include(o => o.Documents)
            .FirstOrDefaultAsync(o => o.Id == regResult.Data.OrganizationId);

        Assert.NotNull(savedOrg);
        Assert.Equal("Lanka Humanitarian Relief Foundation", savedOrg!.Name);
        Assert.Equal("Other", savedOrg.Type);
        Assert.Equal("Regional Disaster Medical Consortium", savedOrg.OtherOrganizationType);
        Assert.Equal("Colombo", savedOrg.District);
        Assert.Equal("Western Province", savedOrg.Province);
        Assert.Contains("Colombo", savedOrg.AreasOfOperation);
        Assert.Contains("Emergency Medical Assistance", savedOrg.AreasOfAssistance);
        Assert.Equal("Psychosocial Trauma Counselling", savedOrg.OtherAreaOfAssistance);
        Assert.Equal("Chamari", savedOrg.RepresentativeFirstName);
        Assert.Equal("Fernando", savedOrg.RepresentativeLastName);
        Assert.Equal("National Humanitarian Director", savedOrg.RepresentativeDesignation);
        Assert.Equal("chamari.f@lankarelief.org", savedOrg.RepresentativeEmail);
        Assert.Equal(2, savedOrg.Documents.Count);
        Assert.False(savedOrg.IsActive);
        Assert.Equal(AccountStatuses.PendingApproval, savedOrg.ApprovalStatus);

        // Test OrganizationService review workflow:
        // 1. Request Additional Information
        var auditLogMock = new Mock<IAuditLogService>();
        var orgService = new OrganizationService(
            _dbContext,
            auditLogMock.Object,
            NullLogger<OrganizationService>.Instance,
            _userManager,
            _roleManager,
            _permissionService,
            _emailMock.Object);

        var requestInfoResult = await orgService.ReviewOrganizationAsync(
            savedOrg.Id,
            new CareLink.Application.DTOs.Organizations.OrganizationReviewDecisionRequestDto
            {
                Decision = "RequestInfo",
                Rationale = "Please confirm cold-chain generator capacity and attach updated annexure."
            },
            "System Administrator");

        Assert.True(requestInfoResult.Success);
        Assert.Equal(AccountStatuses.AdditionalInfoRequested, requestInfoResult.Data!.ApprovalStatus);
        Assert.Equal("Additional Info", requestInfoResult.Data.VerificationStatus);
        Assert.False(requestInfoResult.Data.IsActive);

        // Login while AdditionalInfoRequested must be blocked and include ADDITIONAL_INFO_REQUIRED
        var pendingLogin = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "login.chamari@lankarelief.org",
            Password = "SecurePartnerPassword123!"
        });
        Assert.False(pendingLogin.Success);
        Assert.Contains("ADDITIONAL_INFO_REQUIRED", pendingLogin.Errors!);

        // Representative submits the requested additional information
        var submitInfoResult = await _authService.SubmitPartnerAdditionalInfoAsync(new PartnerAdditionalInfoSubmissionDto
        {
            Email = "login.chamari@lankarelief.org",
            Password = "SecurePartnerPassword123!",
            ResponseText = "We operate two 15kVA diesel generators dedicated to vaccine cold-chain storage.",
            AdditionalDocuments =
            [
                new RegisterDocumentUploadDto
                {
                    Category = "SupportingDocument",
                    FileName = "ColdChain_Spec_Sheet.pdf",
                    ContentType = "application/pdf",
                    FileSizeBytes = pdfBytes.Length,
                    Base64Content = pdfBase64
                }
            ]
        });

        Assert.True(submitInfoResult.Success);

        // Verify document download works for authorized Admin review
        var firstDoc = savedOrg.Documents.First();
        var downloaded = await orgService.GetDocumentForDownloadAsync(savedOrg.Id, firstDoc.Id);
        Assert.NotNull(downloaded);
        Assert.Equal("Lanka_Relief_NGO_Certificate.pdf", downloaded.Value.FileName);

        // 2. Admin Approves Organisation
        var approveOrgResult = await orgService.ReviewOrganizationAsync(
            savedOrg.Id,
            new CareLink.Application.DTOs.Organizations.OrganizationReviewDecisionRequestDto
            {
                Decision = "Approve",
                Rationale = "All registration certificates and cold-chain specifications verified."
            },
            "System Administrator");

        Assert.True(approveOrgResult.Success);
        Assert.True(approveOrgResult.Data!.IsActive);
        Assert.Equal("Verified", approveOrgResult.Data.VerificationStatus);

        // Representative can now log in and receives PartnerOfficer role permissions
        var approvedLogin = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "login.chamari@lankarelief.org",
            Password = "SecurePartnerPassword123!"
        });
        Assert.True(approvedLogin.Success);
        Assert.NotNull(approvedLogin.Data?.Token);
        Assert.Contains(UserRoles.PartnerOfficer, approvedLogin.Data!.User.Roles);
    }

    [Fact]
    public async Task PartnerOrganizationDocumentValidation_RejectsInvalidFileTypes_ExecutablePayloads_AndMissingCertificate()
    {
        var storageService = new OrganizationDocumentStorageService();

        // 1. Disallowed extension (.exe)
        var exeDoc = new RegisterDocumentUploadDto
        {
            Category = "RegistrationCertificate",
            FileName = "malicious.exe",
            ContentType = "application/octet-stream",
            FileSizeBytes = 128,
            Base64Content = Convert.ToBase64String(new byte[] { 0x4D, 0x5A, 0x90, 0x00 })
        };
        var (validExe, exeErr) = storageService.ValidateDocument(exeDoc);
        Assert.False(validExe);
        Assert.Contains("Only PDF, JPG, JPEG, and PNG", exeErr);

        // 2. Disguised MZ executable header with .pdf extension
        var disguisedExe = new RegisterDocumentUploadDto
        {
            Category = "RegistrationCertificate",
            FileName = "certificate.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 128,
            Base64Content = Convert.ToBase64String(new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03 })
        };
        var (validDisguised, disguisedErr) = storageService.ValidateDocument(disguisedExe);
        Assert.False(validDisguised);
        Assert.Contains("signature verification", disguisedErr);

        // 3. Path traversal filename sanitization
        var sanitized = storageService.SanitizeFileName("../../etc/passwd/../../NGO_Cert.pdf");
        Assert.DoesNotContain("..", sanitized);
        Assert.Equal("NGO_Cert.pdf", sanitized);

        // 4. Extended Partner registration without RegistrationCertificate is rejected
        var missingCertReq = new RegisterRequestDto
        {
            Role = "PartnerOfficer",
            FullName = "Test Rep",
            RepresentativeFirstName = "Test",
            RepresentativeLastName = "Rep",
            RepresentativeDesignation = "Director",
            RepresentativeEmail = "rep@testorg.org",
            RepresentativePhone = "+94771112222",
            Email = "rep@testorg.org",
            Password = "Password123!",
            PhoneNumber = "+94771112222",
            OrganizationName = "Test Missing Cert NGO",
            OrganizationType = "Local NGO",
            RegistrationCountry = "Sri Lanka",
            OrganizationEmail = "info@testorg.org",
            OrganizationPhone = "+94112223333",
            OrganizationAddress = "10 Main St, Colombo",
            District = "Colombo",
            Province = "Western Province",
            OrganizationDescription = "Flood relief",
            AreasOfOperation = ["Colombo"],
            AreasOfAssistance = ["Food Distribution"],
            OperationalCapacity = "50 staff",
            EmergencyResponseAvailability = "Available for emergency response",
            ConfirmAuthorisedRepresentative = true,
            ConfirmAccurateInformation = true,
            AgreeTermsAndPrivacy = true,
            Documents = []
        };

        var missingCertResult = await _authService.RegisterAsync(missingCertReq);
        Assert.False(missingCertResult.Success);
        Assert.Contains("Registration Certificate", missingCertResult.Message);
    }

    // =========================================================================
    // 9. ALL 9 OFFICIAL CARELINK ROLES PROVISIONING, PERMISSIONS & JWT CLAIMS
    // =========================================================================

    [Fact]
    public async Task AllNineOfficialRoles_AreDefined_HaveDefaultPermissions_AndAuthenticateWithProperRoleClaims()
    {
        Assert.Equal(9, UserRoles.OfficialRoles.Count);

        var expectedOfficialRoles = new[]
        {
            UserRoles.Admin,
            UserRoles.CaseManager,
            UserRoles.Citizen,
            UserRoles.EmergencyCoordinator,
            UserRoles.FundingOfficer,
            UserRoles.PartnerOfficer,
            UserRoles.ResourceManager,
            UserRoles.ShelterAdministrator,
            UserRoles.VolunteerCoordinator
        };

        foreach (var officialRole in expectedOfficialRoles)
        {
            Assert.Contains(officialRole, UserRoles.OfficialRoles);
            Assert.Contains(officialRole, UserRoles.AllRoles);

            var defaultPerms = DefaultRolePermissions.GetDefaultPermissions(officialRole);
            Assert.NotEmpty(defaultPerms);

            var email = $"{officialRole.ToLowerInvariant()}.verify@carelink.org";
            string? capturedUrl = null;
            _emailMock
                .Setup(e => e.SendAdminCreatedUserEmailAsync(email, $"{officialRole} Officer", It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string, string>((_, _, _, url) => capturedUrl = url)
                .ReturnsAsync(true);

            var createResult = await _userService.CreateUserAsync(new CreateUserRequestDto
            {
                FullName = $"{officialRole} Officer",
                Email = email,
                Role = officialRole,
                IsActive = true
            });

            Assert.True(createResult.Success, $"Failed to create user for role {officialRole}: {createResult.Message}");
            Assert.NotNull(createResult.Data);
            Assert.NotNull(capturedUrl);

            var uri = new Uri(capturedUrl!);
            var queryParts = uri.Query.TrimStart('?').Split('&')
                .Select(p => p.Split('=', 2))
                .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
            var token = queryParts["token"];

            var setupResult = await _authService.SetupPasswordAsync(new SetupPasswordRequestDto
            {
                Email = email,
                Token = token,
                NewPassword = "RoleVerifyPassword123!",
                ConfirmPassword = "RoleVerifyPassword123!"
            });
            Assert.True(setupResult.Success);

            // Authenticate and verify role claim is present
            var loginResult = await _authService.LoginAsync(new LoginRequestDto
            {
                Email = email,
                Password = "RoleVerifyPassword123!"
            });

            Assert.True(loginResult.Success);
            Assert.NotNull(loginResult.Data?.Token);
            Assert.NotEmpty(loginResult.Data!.User.Roles);
            Assert.Equal(AccountStatuses.Active, loginResult.Data.User.AccountStatus);
            Assert.True(loginResult.Data.User.IsActive);
        }
    }
}
