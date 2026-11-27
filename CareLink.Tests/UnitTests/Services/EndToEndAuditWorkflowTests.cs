using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using CareLink.API.Controllers;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Backend.Controllers;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Repositories;
using CareLink.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareLink.Tests.UnitTests.Services;

public class EndToEndAuditWorkflowTests
{
    // =========================================================================
    // 1. CONTROLLER AUTHORIZATION & CITIZEN DATA ISOLATION (IDOR PROTECTION)
    // =========================================================================

    [Fact]
    public void AllOperationalControllers_EnforceAuthorizeAttribute()
    {
        var apiAssembly = typeof(CitizenRequestsController).Assembly;
        var controllerTypes = apiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();

        Assert.True(controllerTypes.Count >= 90, $"Expected at least 90 compiled controllers, found {controllerTypes.Count}.");

        var publicControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HealthController",
            "StatusController",
            "ContactController",
            "LandingController",
            "RequestDemoController",
            "PrivacyController",
            "TermsController",
            "AccessibilityController",
            "ForgotPasswordController"
        };

        foreach (var controller in controllerTypes)
        {
            if (publicControllers.Contains(controller.Name))
                continue;

            var hasClassAuth = controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any();
            var hasActionAuth = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Any(m => m.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any());

            Assert.True(
                hasClassAuth || hasActionAuth,
                $"Controller '{controller.FullName}' is missing [Authorize] protection.");
        }
    }

    [Fact]
    public async Task CitizenRequestsController_EnforcesCitizenDataIsolation_BlocksCrossCitizenIdor_AndAllowsCaseManager()
    {
        var citizenRequestService = new CitizenRequestService(new InMemoryCitizenRequestRepository());
        var myCasesService = new MyCasesService();
        var caseManagementService = new CaseManagementService();
        var assistanceRepo = new InMemoryAssistanceRequestRepository();

        var controller = new CitizenRequestsController(
            citizenRequestService,
            myCasesService,
            caseManagementService,
            assistanceRepo);

        // Citizen A ("Nithya Hansani") creates a request
        SetControllerUser(controller, "user-citizen-a", "Nithya Hansani", "citizen@carelink.org", UserRoles.Citizen);

        var createResponse = await controller.CreateRequest(new CreateCitizenHelpRequestPayload
        {
            Title = "Flood evacuation boat needed",
            Category = "Rescue",
            Priority = "Urgent",
            Address = "North Ward, Lane 3",
            PeopleCount = 5,
            Description = "Water level rising rapidly inside ground floor.",
            CitizenName = "Spoofed Citizen Name", // Controller overrides with authenticated citizen name
            Phone = "+94 77 111 2233",
            ImmediateThreat = true
        }, null);

        var createdResult = Assert.IsType<CreatedAtActionResult>(createResponse);
        var createdPayload = Assert.IsType<CareLink.Application.Common.ApiResponse<CitizenHelpRequestDto>>(createdResult.Value);
        Assert.True(createdPayload.Success);
        var requestId = createdPayload.Data!.Id;
        Assert.Equal("Nithya Hansani", createdPayload.Data.CitizenName);

        // Citizen B ("Other Citizen") attempts to view Citizen A's request by ID -> 403 Forbidden
        SetControllerUser(controller, "user-citizen-b", "Other Citizen", "other.citizen@carelink.org", UserRoles.Citizen);

        var crossViewById = await controller.GetById(requestId);
        var forbidById = Assert.IsType<ObjectResult>(crossViewById);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidById.StatusCode);

        // Citizen B attempts to query Citizen A's requests via ?citizenName=Nithya Hansani -> 403 Forbidden
        var crossQuery = await controller.GetRequests("Nithya Hansani", null, null, null);
        var forbidQuery = Assert.IsType<ObjectResult>(crossQuery);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidQuery.StatusCode);

        // Citizen B attempts to delete Citizen A's request -> 403 Forbidden
        var crossDelete = await controller.DeleteRequest(requestId);
        var forbidDelete = Assert.IsType<ObjectResult>(crossDelete);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidDelete.StatusCode);

        // Case Manager CAN view Citizen A's request
        SetControllerUser(controller, "user-cm-1", "Amara Perera", "casemanager@carelink.org", UserRoles.CaseManager);
        var cmView = await controller.GetById(requestId);
        var cmOk = Assert.IsType<OkObjectResult>(cmView);
        var cmPayload = Assert.IsType<CareLink.Application.Common.ApiResponse<CitizenHelpRequestDto>>(cmOk.Value);
        Assert.True(cmPayload.Success);
        Assert.Equal(requestId, cmPayload.Data!.Id);
    }

    // =========================================================================
    // 2. WORKFLOW A: CITIZEN REQUEST -> CASE MANAGER REVIEW -> CITIZEN STATUS SYNC
    // =========================================================================

    [Fact]
    public async Task WorkflowA_CitizenCreatesHelpRequest_PropagatesToCaseManagerAndCoordinator_AndStatusUpdatesSyncBack()
    {
        var citizenRequestService = new CitizenRequestService(new InMemoryCitizenRequestRepository());
        var myCasesService = new MyCasesService();
        var caseManagementService = new CaseManagementService();
        var assistanceRepo = new InMemoryAssistanceRequestRepository();

        var citizenController = new CitizenRequestsController(
            citizenRequestService,
            myCasesService,
            caseManagementService,
            assistanceRepo);

        SetControllerUser(citizenController, "citizen-01", "Nithya Hansani", "citizen@carelink.org", UserRoles.Citizen);

        // Step 1: Citizen creates assistance request
        var createRes = await citizenController.CreateRequest(new CreateCitizenHelpRequestPayload
        {
            Title = "Emergency Infant Formula & Clean Water",
            Category = "Food & Water",
            Priority = "Urgent",
            Address = "Riverside, Ward 2",
            PeopleCount = 4,
            Description = "Stranded with two infants needing clean water and formula.",
            Phone = "+94 77 456 7890",
            ImmediateThreat = true
        }, null);

        var createdObj = Assert.IsType<CreatedAtActionResult>(createRes);
        var createdDto = Assert.IsType<CareLink.Application.Common.ApiResponse<CitizenHelpRequestDto>>(createdObj.Value).Data!;
        var requestId = createdDto.Id;

        // Step 2: Verify propagation to Case Manager (MyCases & CaseManagement) and Coordinator (AssistanceRequests)
        var cmMyCase = await myCasesService.GetCaseByIdAsync(requestId);
        Assert.NotNull(cmMyCase);
        Assert.Contains("Nithya Hansani", cmMyCase!.Applicant);

        var cmManagedCase = await caseManagementService.GetCaseByIdAsync(requestId);
        Assert.NotNull(cmManagedCase);
        Assert.Contains("Nithya Hansani", cmManagedCase!.Applicant);

        var coordReq = await assistanceRepo.GetByIdAsync(requestId);
        Assert.NotNull(coordReq);
        Assert.Equal("Riverside, Ward 2", coordReq!.Location);

        // Step 3: Case Manager updates case status to "In Progress" and then "Resolved"
        var myCasesController = new MyCasesController(
            myCasesService,
            NullLogger<MyCasesController>.Instance,
            caseManagementService,
            citizenRequestService,
            assistanceRepo);

        SetControllerUser(myCasesController, "cm-01", "Amara Perera", "casemanager@carelink.org", UserRoles.CaseManager);

        var inProgressResult = await myCasesController.UpdateStatus(requestId, new UpdateMyCaseStatusRequest
        {
            Status = "In Progress",
            Priority = "Critical",
            Notes = "Dispatched field relief unit with water and formula."
        });
        Assert.IsType<OkObjectResult>(inProgressResult.Result);

        var citizenViewInProgress = await citizenRequestService.GetByIdAsync(requestId);
        Assert.Equal("In Progress", citizenViewInProgress.Data!.Status);

        var resolvedResult = await myCasesController.UpdateStatus(requestId, new UpdateMyCaseStatusRequest
        {
            Status = "Resolved",
            Priority = "Critical",
            Notes = "Relief package delivered and confirmed with beneficiary."
        });
        Assert.IsType<OkObjectResult>(resolvedResult.Result);

        // Step 4: Verify Citizen and Coordinator both see "Resolved"
        var citizenViewResolved = await citizenRequestService.GetByIdAsync(requestId);
        Assert.Equal("Resolved", citizenViewResolved.Data!.Status);

        var coordViewResolved = await assistanceRepo.GetByIdAsync(requestId);
        Assert.Equal("Resolved", coordViewResolved!.Status);
    }

    // =========================================================================
    // 3. WORKFLOW C: VOLUNTEER COORDINATION & UNAVAILABLE VOLUNTEER REJECTION
    // =========================================================================

    [Fact]
    public async Task WorkflowC_VolunteerAssignment_AllowsAvailableVolunteer_AndRejectsUnavailableVolunteer()
    {
        var directoryService = new VolunteerDirectoryService();
        var assignmentsService = new AssignmentsService(directoryService);
        var controller = new AssignmentsController(assignmentsService, NullLogger<AssignmentsController>.Instance);
        SetControllerUser(controller, "vc-01", "Nadia Okafor", "volunteercoordinator@carelink.org", UserRoles.VolunteerCoordinator);

        // 1. Assigning an AVAILABLE volunteer (Elena Rossi - Available now) succeeds
        var validCreate = await controller.CreateAssignment(new CreateAssignmentRequest
        {
            Volunteer = "Elena Rossi",
            Task = "Medical Triage Support",
            Location = "North Ward",
            Schedule = "14:00–20:00",
            Notes = "Critical flood triage deployment"
        });

        var okResult = Assert.IsType<OkObjectResult>(validCreate.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        // 2. Assigning an UNAVAILABLE volunteer (Daniel Okafor - Unavailable) is rejected with 400 BadRequest
        var invalidCreate = await controller.CreateAssignment(new CreateAssignmentRequest
        {
            Volunteer = "Daniel Okafor",
            Task = "Flood Evacuation Support",
            Location = "Riverside",
            Schedule = "15:00–21:00",
            Notes = "Attempted assignment of unavailable volunteer"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(invalidCreate.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    // =========================================================================
    // 4. WORKFLOW D: RESOURCE ALLOCATION STOCK VALIDATION & DEDUCTION
    // =========================================================================

    [Fact]
    public async Task WorkflowD_ResourceAllocation_DeductsInventoryStock_AndRejectsInsufficientStock()
    {
        var inventoryRepo = new InMemoryInventoryRepository();
        var allocationRepo = new InMemoryAllocationRepository();
        var allocationService = new AllocationService(allocationRepo, inventoryRepo);

        var initialItem = await inventoryRepo.GetByIdAsync("RES-00128");
        Assert.NotNull(initialItem);
        var initialStock = initialItem!.Available;
        Assert.True(initialStock >= 120);

        // 1. Valid allocation of 120 units deducts 120 units from inventory
        var createdAllocation = await allocationService.CreateAllocationAsync(new CreateAllocationRequest
        {
            Request = "REQ-4829",
            Resource = "Drinking Water - 10L",
            Quantity = "120 Units",
            Source = "Central Depot",
            Destination = "Riverton Central School",
            ApprovedBy = "Resource Manager"
        });

        Assert.True(createdAllocation.Success);
        Assert.NotNull(createdAllocation.Data);
        var updatedItem = await inventoryRepo.GetByIdAsync("RES-00128");
        Assert.Equal(initialStock - 120, updatedItem!.Available);

        // 2. Allocation exceeding available stock returns ErrorResponse
        var excessiveResult = await allocationService.CreateAllocationAsync(new CreateAllocationRequest
        {
            Request = "REQ-4830",
            Resource = "Drinking Water - 10L",
            Quantity = "999999 Units",
            Source = "Central Depot",
            Destination = "North Community Hall"
        });

        Assert.False(excessiveResult.Success);
        Assert.Contains("Insufficient available stock", excessiveResult.Message, StringComparison.OrdinalIgnoreCase);

        // 3. Zero or negative quantity is rejected
        var zeroResult = await allocationService.CreateAllocationAsync(new CreateAllocationRequest
        {
            Request = "REQ-4831",
            Resource = "Drinking Water - 10L",
            Quantity = "0 Units",
            Source = "Central Depot",
            Destination = "North Community Hall"
        });

        Assert.False(zeroResult.Success);
        Assert.Contains("greater than zero", zeroResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // 5. WORKFLOW E: SHELTER CAPACITY ENFORCEMENT & STRING CODE RESOLUTION
    // =========================================================================

    [Fact]
    public async Task WorkflowE_ShelterOccupancy_EnforcesMaximumCapacity_AndResolvesStringShelterCodes()
    {
        var options = new DbContextOptionsBuilder<CareLinkDbContext>()
            .UseInMemoryDatabase($"ShelterWorkflowTestDb_{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new CareLinkDbContext(options);
        await DataSeeder.SeedAsync(dbContext);

        var shelterRepo = new ShelterRepository(dbContext);
        var shelterService = new ShelterService(shelterRepo);
        var occupancyRepo = new OccupancyRepository(dbContext);
        var occupancyService = new OccupancyService(occupancyRepo);
        var sheltersController = new SheltersController(shelterService, NullLogger<SheltersController>.Instance);
        SetControllerUser(sheltersController, "sa-01", "Sanjana Amarasinghe", "shelteradmin@carelink.org", UserRoles.ShelterAdministrator);

        // 1. Resolve shelter by string code "SH-002" (North Community Hall: Capacity 500, Occupied 284)
        var getSh002 = await sheltersController.GetById("SH-002", CancellationToken.None);
        var okSh002 = Assert.IsType<OkObjectResult>(getSh002);
        var sh002Dto = Assert.IsType<ShelterDto>(okSh002.Value);
        Assert.Equal("North Community Hall", sh002Dto.Name);

        // 2. Valid admission within capacity via OccupancyService ("SH-002" has > 10 available spaces)
        var validAdmission = await occupancyService.RecordAdmissionAsync(new RecordAdmissionDto
        {
            Resident = "Jayawardena Family",
            ShelterId = "SH-002",
            People = 6,
            Date = DateTime.UtcNow.ToString("yyyy-MM-dd")
        });
        Assert.NotNull(validAdmission);
        Assert.Equal(6, validAdmission.People);

        // 3. Exceeding capacity on "SH-003" (St. Mary Civic Centre: 450/450 Full) is rejected
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            occupancyService.RecordAdmissionAsync(new RecordAdmissionDto
            {
                Resident = "Overflow Group",
                ShelterId = "SH-003",
                People = 5,
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd")
            }));

        // 4. Attempting to patch occupancy above capacity via SheltersController returns 400 BadRequest
        var overCapacityPatch = await sheltersController.UpdateOccupancy(
            "SH-002",
            new UpdateShelterOccupancyDto { Occupied = 9999 },
            CancellationToken.None);

        var badPatch = Assert.IsType<BadRequestObjectResult>(overCapacityPatch.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badPatch.StatusCode);
    }

    // =========================================================================
    // 6. FULL HTTP PIPELINE & ALL 9 CANONICAL ROLES E2E INTEGRATION TEST
    // =========================================================================

    [Fact]
    public async Task FullHttpPipeline_AllNineRolesLogin_RbacEnforcement_AndUnifiedModuleEndpointsSucceed()
    {
        await using var sqliteConn = new SqliteConnection("DataSource=:memory:");
        await sqliteConn.OpenAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<ApplicationDbContext>();

                    var sqliteOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlite(sqliteConn)
                        .Options;

                    services.AddSingleton(sqliteOptions);
                    services.AddScoped<ApplicationDbContext>( _ => new ApplicationDbContext(sqliteOptions));
                });
            });

        using var client = factory.CreateClient();

        // 1. Health Check endpoint is public and returns 200 OK
        var healthRes = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, healthRes.StatusCode);

        // 2. Unauthenticated requests to protected endpoints return 401 Unauthorized
        var unauthEndpoints = new[]
        {
            "/api/users",
            "/api/citizen/requests",
            "/api/mycases",
            "/api/case-manager/priority-queue",
            "/api/coordinator/priority-queue",
            "/api/case-manager/notifications",
            "/api/assignments",
            "/api/allocations",
            "/api/shelters",
            "/api/donations",
            "/api/partner-dashboard"
        };

        foreach (var endpoint in unauthEndpoints)
        {
            var res = await client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        // 3. Log in with all 9 Canonical Role accounts (+ Volunteer) seeded by DbInitializer
        var roleCredentials = new (string Role, string Email, string Password, string ProtectedEndpoint)[]
        {
            (UserRoles.Admin, "admin@carelink.org", "Admin@123456", "/api/users"),
            (UserRoles.CaseManager, "casemanager@carelink.org", "CaseManager@123456", "/api/mycases"),
            (UserRoles.Citizen, "citizen@carelink.org", "Citizen@123456", "/api/citizen/requests"),
            (UserRoles.EmergencyCoordinator, "coordinator@carelink.org", "Coordinator@123456", "/api/coordinator/priority-queue"),
            (UserRoles.FundingOfficer, "funding@carelink.org", "Funding@123456", "/api/donations"),
            (UserRoles.PartnerOfficer, "partner@carelink.org", "Partner@123456", "/api/partner-dashboard"),
            (UserRoles.ResourceManager, "resourcemanager@carelink.org", "Resource@123456", "/api/inventory"),
            (UserRoles.ShelterAdministrator, "shelteradmin@carelink.org", "Shelter@123456", "/api/shelters"),
            (UserRoles.VolunteerCoordinator, "volunteercoordinator@carelink.org", "VolunteerCoord@123456", "/api/assignments")
        };

        var tokensByRole = new Dictionary<string, string>();

        foreach (var (role, email, password, endpoint) in roleCredentials)
        {
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
            {
                Email = email,
                Password = password
            });

            Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
            var json = await loginRes.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.GetProperty("success").GetBoolean(), $"Login failed for role {role} ({email}).");
            var token = json.GetProperty("data").GetProperty("token").GetString();
            Assert.False(string.IsNullOrWhiteSpace(token));
            tokensByRole[role] = token!;

            // Verify authenticated GET to the role's primary endpoint returns 200 OK
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var endpointRes = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, endpointRes.StatusCode);
        }

        // 4. Verify RBAC forbids Citizen from accessing Admin /api/users endpoint (403 Forbidden)
        using var citizenToAdminReq = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        citizenToAdminReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokensByRole[UserRoles.Citizen]);
        var forbiddenRes = await client.SendAsync(citizenToAdminReq);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenRes.StatusCode);

        // 5. Verify newly unified CaseManager, VolunteerCoordinator, and ShelterReports HTTP routes
        var unifiedRoutes = new (string Role, string Route)[]
        {
            (UserRoles.CaseManager, "/api/casemanagement/dashboard"),
            (UserRoles.CaseManager, "/api/casemanagement/cases"),
            (UserRoles.CaseManager, "/api/case-manager/priority-queue"),
            (UserRoles.CaseManager, "/api/case-manager/notifications"),
            (UserRoles.CaseManager, "/api/mycases/stats"),
            (UserRoles.VolunteerCoordinator, "/api/volunteer/dashboard"),
            (UserRoles.VolunteerCoordinator, "/api/communications/broadcasts"),
            (UserRoles.VolunteerCoordinator, "/api/volunteer/notifications"),
            (UserRoles.ShelterAdministrator, "/api/shelter/reports/overview"),
            (UserRoles.EmergencyCoordinator, "/api/assistancerequests"),
            (UserRoles.EmergencyCoordinator, "/api/assistance-requests")
        };

        foreach (var (role, route) in unifiedRoutes)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, route);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokensByRole[role]);
            var res = await client.SendAsync(req);
            Assert.True(
                res.StatusCode == HttpStatusCode.OK,
                $"Expected 200 OK for route '{route}' with role '{role}', got {(int)res.StatusCode} {res.StatusCode}.");
        }
    }

    private static void SetControllerUser(
        ControllerBase controller,
        string userId,
        string fullName,
        string email,
        string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, fullName),
            new("fullName", fullName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestJwtAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
