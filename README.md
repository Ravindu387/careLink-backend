# CareLink – Humanitarian Crisis Management System (Backend API)

Welcome to the backend repository of **CareLink**, an enterprise-grade humanitarian crisis management platform designed to coordinate disaster response, volunteer mobilization, emergency shelter management, resource distribution, transparent funding allocations, and AI-assisted decision support.

The CareLink backend is developed with **ASP.NET Core (.NET 10)** following the **Clean Architecture** pattern. It provides high-performance, secure REST APIs powering the CareLink React (Vite) frontend application.

---

## Table of Contents

- [1. Project Overview](#1-project-overview)
- [2. Technology Stack](#2-technology-stack)
- [3. Architecture](#3-architecture)
  - [Solution Structure](#solution-structure)
  - [Folder & Layer Layout](#folder--layer-layout)
- [4. Prerequisites](#4-prerequisites)
- [5. Installation and Setup](#5-installation-and-setup)
  - [Clone Repository](#clone-repository)
  - [Restore and Build](#restore-and-build)
  - [Database Setup & Seeding](#database-setup--seeding)
- [6. Environment Configuration and Secrets](#6-environment-configuration-and-secrets)
  - [Configuration Precedence](#configuration-precedence)
  - [Local Secrets with .NET User Secrets](#local-secrets-with-net-user-secrets)
  - [Configuration Keys Reference](#configuration-keys-reference)
- [7. Running the Backend](#7-running-the-backend)
  - [Launch Commands](#launch-commands)
  - [Health Check Endpoint](#health-check-endpoint)
- [8. Swagger & OpenAPI Documentation](#8-swagger--openapi-documentation)
  - [Swagger UI Access](#swagger-ui-access)
  - [Using JWT Authentication in Swagger](#using-jwt-authentication-in-swagger)
- [9. Authentication & Authorization](#9-authentication--authorization)
  - [Authentication Flow](#authentication-flow)
  - [Supported User Roles](#supported-user-roles)
  - [Permission-Based Authorization](#permission-based-authorization)
  - [Administrative Single-Use Setup Tokens](#administrative-single-use-setup-tokens)
- [10. API Modules & Endpoints Catalog](#10-api-modules--endpoints-catalog)
  - [Authentication & Identity](#authentication--identity)
  - [User & Organization Management](#user--organization-management)
  - [AI & Recommendation Services](#ai--recommendation-services)
  - [Chatbot Services](#chatbot-services)
  - [Emergency & Incident Management](#emergency--incident-management)
  - [Assistance Requests & Case Management](#assistance-requests--case-management)
  - [Volunteer Management](#volunteer-management)
  - [Shelter & Accommodation Management](#shelter--accommodation-management)
  - [Resource & Warehouse Logistics](#resource--warehouse-logistics)
  - [Donations & Financial Tracking](#donations--financial-tracking)
  - [Audit Logging & Timeline](#audit-logging--timeline)
- [11. Database Architecture](#11-database-architecture)
  - [Hybrid Persistence Architecture](#hybrid-persistence-architecture)
  - [ApplicationDbContext (SQL Server)](#applicationdbcontext-sql-server)
  - [CareLinkDbContext (EF Core In-Memory)](#carelinkdbcontext-ef-core-in-memory)
  - [In-Memory Repositories](#in-memory-repositories)
- [12. AI Integration (Google Gemini)](#12-ai-integration-google-gemini)
  - [AI Decision-Support Capabilities](#ai-decision-support-capabilities)
  - [Human-in-the-Loop Review Workflow](#human-in-the-loop-review-workflow)
  - [Deterministic Fallback Engine](#deterministic-fallback-engine)
- [13. Email & SMTP Integration](#13-email--smtp-integration)
- [14. Testing](#14-testing)
  - [Running the Test Suite](#running-the-test-suite)
  - [Verified Test Results](#verified-test-results)
- [15. Frontend Integration](#15-frontend-integration)
- [16. Known Limitations](#16-known-limitations)
- [17. Future Improvements](#17-future-improvements)
- [18. Troubleshooting](#18-troubleshooting)

---

## 1. Project Overview

During natural disasters and humanitarian emergencies (e.g., severe floods, landslides, extreme weather events), real-time coordination between government agencies, NGOs, volunteers, and displaced citizens is critical.

**CareLink** addresses coordination bottlenecks by delivering:
1. **Multi-Role Crisis Platform:** Dedicated operational portals for System Administrators, Incident Managers, Emergency Coordinators, Case Managers, Volunteer Coordinators, Shelter Administrators, Resource Managers, Funding Officers, Partner NGOs, and Affected Citizens.
2. **Citizen Self-Service:** Rapid submission of assistance requests, emergency evacuation needs, real-time status tracking, and nearby shelter discovery.
3. **AI-Assisted Humanitarian Intelligence:** AI analysis powered by Google Gemini to triage urgent citizen requests, evaluate shelter capacity, compute volunteer skill-to-task matching, and optimize supply distribution with mandatory human authorization.
4. **Transparent Resource & Financial Accountability:** End-to-end ledger tracking for incoming public donations, NGO expenditures, warehouse stock movements, and audit log generation.
5. **Decoupled API Architecture:** The ASP.NET Core backend exposes a comprehensive JSON REST API consumed by a modern React (Vite) Single-Page Application (SPA).

---

## 2. Technology Stack

| Layer / Concern | Technology | Version | Purpose |
| :--- | :--- | :--- | :--- |
| **Framework** | ASP.NET Core / .NET | `10.0` (`net10.0`) | High-performance Web API hosting and dependency injection |
| **Language** | C# | `13 / 14` | Modern C# language features, nullable reference types, pattern matching |
| **ORM** | Entity Framework Core | `10.0.0` | Object-relational mapping, LINQ queries, database migrations |
| **Primary Database** | Microsoft SQL Server | 2019 / 2022 / LocalDB | Persistent storage for Identity, Users, Organizations, Documents, Audit Logs, and AI Recommendations |
| **Operational DB** | EF Core In-Memory | `10.0.0` | High-throughput in-memory state engine for shelter occupancy, live warehouse inventory, and simulations |
| **Identity & Security** | ASP.NET Core Identity | `10.0.0` | User account management, PBKDF2 password hashing, role management |
| **Authentication** | JWT Bearer Authentication | `10.0.0` | Stateless HMAC-SHA256 tokens, claims-based authorization, token expiry validation |
| **AI Integration** | Google Gemini API | `v1beta` (HTTP) | Automated crisis analysis, recommendation generation, conversational grounding (`gemini-1.5-flash`) |
| **Email / SMTP** | `System.Net.Mail` | Built-in | Gmail SMTP with TLS/STARTTLS, single-use invite tokens, and delivery diagnostic categorizers |
| **API Documentation** | `Microsoft.AspNetCore.OpenApi` + `Swashbuckle.AspNetCore.SwaggerUI` | `10.0.12` / `11.0.1` | OpenAPI v3 schema generation and interactive Swagger UI at `/swagger` |
| **Testing** | xUnit, Moq, EF Core InMemory | `2.9.3` / `4.20.72` | Automated unit testing, repository mocking, in-process end-to-end workflow verification |

---

## 3. Architecture

The CareLink backend strictly enforces the **Clean Architecture (Onion Architecture)** pattern, decoupling domain logic from external frameworks, databases, and network transports.

```
       +-------------------------------------------------------+
       |                     CareLink.API                      |
       |  (Controllers, Middleware, Auth Handlers, Swagger UI) |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |                 CareLink.Application                  |
       |     (DTOs, Service Interfaces, Logic, Validation)     |
       +---------------------------+---------------------------+
                                   |
         +-------------------------+-------------------------+
         |                                                   |
         v                                                   v
+-------------------------------+         +-------------------------------+
|         CareLink.Core         |         |    CareLink.Infrastructure    |
| (Entities, Constants, Enums)  |         | (EF Core, DbContext, Gemini,  |
|   [Zero external libraries]   |         |    EmailService, Repositories)|
+-------------------------------+         +-------------------------------+
```

### Solution Structure

The solution file is [CareLink.slnx](file:///d:/NSBM/SA-Project/careLink-backend/CareLink.slnx) and consists of:

* **`CareLink.Core`**: The innermost domain layer. Contains domain entities (`ApplicationUser`, `Organization`, `OrganizationDocument`, `ChatbotConversation`, `ChatbotMessage`, `VerificationRequest`, etc.), system-wide role constants (`UserRoles`), and granular permission definitions (`Permissions`). It has zero dependencies on any database, UI, or external package.
* **`CareLink.Application`**: The application business rules layer. Houses all Data Transfer Objects (DTOs), service interfaces (`IAuthService`, `IUserService`, `IChatbotService`, `IVerificationService`, `IAiRecommendationService`), business logic implementations, and standardized API response wrappers (`ApiResponse<T>`).
* **`CareLink.Infrastructure`**: The external mechanisms layer. Implements SQL Server persistence via `ApplicationDbContext`, in-memory operations via `CareLinkDbContext`, database seeders (`DbInitializer`, `DataSeeder`), email delivery via `EmailService`, and the Google Gemini AI connector via `GeminiService`.
* **`CareLink.API`**: The entry point and presentation layer. Contains ASP.NET Core controllers, JWT Bearer configuration, CORS policies, authorization requirement handlers (`PermissionAuthorizationHandler`), middleware pipeline, and OpenAPI/Swagger UI endpoints.
* **`CareLink.Tests`**: The verification layer containing automated xUnit tests validating authentication, permission gates, single-use invite tokens, AI recommendation workflows, and email fallback behaviors.
* **`CareLink.Backend.csproj` (Root Links)**: Houses specialized Case Management and Volunteer Coordination services and controllers linked directly into the solution for operational modularity.

### Folder & Layer Layout

```
careLink-backend/
|-- CareLink.slnx                       # Solution definition
|-- Directory.Build.props               # Solution-wide compiler & analyzer configurations
|-- CareLink.Core/                      # Domain Entities, Constants, & Interfaces
|   |-- Constants/                      # UserRoles, Permissions, AccountStatuses, DefaultRolePermissions
|   |-- Entities/                       # ApplicationUser, Organization, VerificationRequest, Chatbot, etc.
|   +-- Interfaces/                     # Domain repository contracts
|-- CareLink.Application/               # Application Business Logic
|   |-- Common/                         # ApiResponse<T>, EmailSettings, EmailDeliveryResult, GeminiOptions
|   |-- DTOs/                           # Auth, Users, Organizations, Verification, Chatbot, Audit, AI DTOs
|   |-- Interfaces/                     # Service contracts (IAuthService, IUserService, IChatbotService, etc.)
|   +-- Services/                       # AuthService, UserService, ChatbotService, VerificationService, etc.
|-- CareLink.Infrastructure/            # Data Access & External Services
|   |-- Data/                           # ApplicationDbContext (SQL Server), CareLinkDbContext, Seeders
|   |-- Repositories/                   # ChatbotRepository, ShelterRepository, InMemory Repositories
|   +-- Services/                       # EmailService, EmailTemplates, GeminiService, RoleService
|-- CareLink.API/                       # Web API Presentation Layer
|   |-- Authorization/                  # PermissionAuthorizationHandler & Requirements
|   |-- Controllers/                    # 60+ REST API Controllers across all crisis domains
|   |-- Properties/                     # launchSettings.json (Port 5277 / 7113)
|   |-- appsettings.json                # Base configuration template
|   |-- appsettings.Development.json    # Development overrides
|   +-- Program.cs                      # Dependency Injection, Middleware, & Endpoint Routing
|-- CareLink.Tests/                     # Test Suite
|   |-- Common/                         # FakeGeminiService, Test doubles
|   +-- UnitTests/Services/             # UserManagementAndAuthTests, ChatbotServiceTests, AuditWorkflowTests
+-- Controllers/, Services/, Models/    # Root modules for Case Management & Volunteer Coordination
```

---

## 4. Prerequisites

Before installing and running the CareLink backend, verify that your development machine has the following tools installed:

1. **.NET 10 SDK (Version 10.0.100 or later)**
   * Verify via terminal:
     ```bash
     dotnet --version
     ```
2. **Microsoft SQL Server**
   * Any of the following editions:
     * SQL Server LocalDB (included with Visual Studio, e.g., `(localdb)\mssqllocaldb`)
     * SQL Server Express 2019 / 2022
     * SQL Server Developer Edition / Docker SQL Server container
3. **Database Client Tool**
   * SQL Server Management Studio (SSMS), Azure Data Studio, or JetBrains DataGrip.
4. **IDE / Editor**
   * Visual Studio 2022 / 2025 (with ASP.NET and web development workload), or Visual Studio Code with the **C# Dev Kit** extension.
5. **Git**
   * For cloning and version control.

---

## 5. Installation and Setup

### Clone Repository

```bash
git clone https://github.com/Ravindu387/careLink-backend.git
cd careLink-backend
```

### Restore and Build

Restore NuGet dependencies and build all projects in the solution:

```bash
dotnet restore CareLink.slnx
dotnet build CareLink.slnx
```

Verify that the build completes with `0 Error(s)`.

### Database Setup & Seeding

CareLink uses **Entity Framework Core Code-First** with **automatic database migration and seeders** executed on application startup:

1. **Automatic Initialization:** When `CareLink.API` starts in the Development environment, [Program.cs](file:///d:/NSBM/SA-Project/careLink-backend/CareLink.API/Program.cs#L257-L287) calls:
   * `DbInitializer.SeedAsync()`: Automatically creates the SQL Server database schema (if not created), seeds default roles, permissions, administrative staff accounts, and initial organizations.
   * `DataSeeder.SeedAsync()`: Populates the in-memory `CareLinkDbContext` with active disaster events, demo shelters, emergency contacts, and sample stock items.
2. **Manual Migration Execution (Optional):**
   If you wish to apply EF Core migrations explicitly using the CLI:
   ```bash
   dotnet ef database update --project CareLink.Infrastructure --startup-project CareLink.API
   ```

---

## 6. Environment Configuration and Secrets

### Configuration Precedence

CareLink resolves configuration settings in the following strict order (last wins):
1. `CareLink.API/appsettings.json` (Base configurations)
2. `CareLink.API/appsettings.Development.json` (Local dev configurations)
3. `CareLink.API/appsettings.local.json` (Optional local developer override, untracked by Git)
4. **.NET User Secrets** (Secure local machine secret storage)
5. **Environment Variables** (Docker / Host system environment variables)

> [!IMPORTANT]
> **Never commit real passwords, API keys, or JWT private keys to Git.** Store sensitive credentials using `.NET User Secrets` during local development.

### Local Secrets with .NET User Secrets

To initialize and populate User Secrets for `CareLink.API`:

```bash
cd CareLink.API

# Set Google Gemini API Key
dotnet user-secrets set "Gemini:ApiKey" "<YOUR_GEMINI_API_KEY>"

# Set Gmail SMTP 16-Character App Password
dotnet user-secrets set "EmailSettings:Password" "<YOUR_16_CHAR_GMAIL_APP_PASSWORD>"

# Verify configured secrets
dotnet user-secrets list
```

### Configuration Keys Reference

Below is the structure of `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CareLinkIdentityDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "CareLinkSecureEmergencyKeyForDevEnvironment2026",
    "Issuer": "CareLinkAPI",
    "Audience": "CareLinkClient",
    "ExpiryHours": "24"
  },
  "EmailSettings": {
    "SenderName": "CareLink Admin",
    "SenderEmail": "carelink.platform.admin@gmail.com",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "carelink.platform.admin@gmail.com",
    "Password": "",
    "EnableSsl": true,
    "SetupTokenExpiryHours": 24,
    "VerificationBaseUrl": "http://localhost:5277/api/auth/verify-email",
    "FrontendBaseUrl": "http://localhost:5173",
    "PasswordSetupBaseUrl": "http://localhost:5173/setup-account"
  },
  "Gemini": {
    "ApiKey": "",
    "Model": "gemini-1.5-flash",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta",
    "TimeoutSeconds": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 7. Running the Backend

### Launch Commands

To start the API in Development mode:

```bash
dotnet run --project CareLink.API
```

Once started, the backend listens on:
* **HTTP:** `http://localhost:5277`
* **HTTPS:** `https://localhost:7113`

### Health Check Endpoint

Verify that the backend is running and operational by querying the health endpoint:

```bash
curl http://localhost:5277/api/health
```

**Expected JSON Response:**
```json
{
  "status": "Healthy",
  "application": "CareLink Humanitarian Crisis API",
  "version": "1.0.0",
  "timestamp": "2026-11-27T17:35:06.2996572Z"
}
```

---

## 8. Swagger & OpenAPI Documentation

### Swagger UI Access

CareLink provides interactive API documentation powered by `Swashbuckle.AspNetCore.SwaggerUI` and `Microsoft.AspNetCore.OpenApi`:

* **Interactive Swagger UI:** [http://localhost:5277/swagger](http://localhost:5277/swagger)
* **Raw OpenAPI v3 JSON Schema:** [http://localhost:5277/openapi/v1.json](http://localhost:5277/openapi/v1.json)

> [!NOTE]
> Swagger UI is enabled by default when the backend runs in the `Development` environment (`ASPNETCORE_ENVIRONMENT=Development`).

### Using JWT Authentication in Swagger

Protected endpoints require a valid JWT Bearer token:
1. Call `POST /api/auth/login` in Swagger or via curl with valid administrative credentials.
2. Copy the returned `token` from the response.
3. For endpoints that require authorization, supply the HTTP header:
   ```http
   Authorization: Bearer <YOUR_JWT_TOKEN>
   ```

---

## 9. Authentication & Authorization

### Authentication Flow

CareLink implements stateless **JWT Bearer Authentication**:
1. **Public Registration:** Citizens, Volunteers, and Partner NGO Officers can register through `POST /api/auth/register`.
2. **Staff Account Invitations:** Administrators invite operational staff (e.g. Case Managers, Emergency Coordinators) using `POST /api/users`. An email setup invitation with a secure single-use token is delivered via Gmail SMTP.
3. **Password Configuration:** Invited users configure their password at `http://localhost:5173/setup-account` which calls `POST /api/auth/setup-password`.
4. **Token Generation:** On successful authentication via `POST /api/auth/login`, `IJwtTokenGenerator` issues a signed JWT containing user claims (`sub`, `email`, `role`, and assigned granular `permission` claims).
5. **Token Lifetime:** Configurable via `Jwt:ExpiryHours` (default: 24 hours).

### Supported User Roles

Defined in [UserRoles.cs](file:///d:/NSBM\SA-Project\careLink-backend\CareLink.Core\Constants\UserRoles.cs):

#### Official 9 System Roles
1. `Admin`: Master platform administrator with full access to user management, system settings, security logs, and role provisioning.
2. `CaseManager`: Oversees citizen assistance triage, case verification, and emergency referrals.
3. `Citizen`: Affected individuals submitting assistance requests, locating shelters, and interacting with the AI crisis assistant.
4. `EmergencyCoordinator`: High-level operational disaster lead overseeing live maps, situation reports, and crisis declarations.
5. `FundingOfficer`: Manages incoming donor pledges, transparent fund allocations, and financial expenditure audits.
6. `PartnerOfficer`: Verified NGO and agency representatives coordinating joint field missions and supply transfers.
7. `ResourceManager`: Oversees warehouse inventory, procurement queues, stock movements, and relief dispatches.
8. `ShelterAdministrator`: Manages emergency shelters, room/bed assignments, occupancy thresholds, and facility maintenance.
9. `VolunteerCoordinator`: Directs volunteer mobilization, skill-to-task assignments, and field verification.

#### Backward-Compatible Identity Role Aliases
* `SystemAdmin` (Alias for `Admin`)
* `IncidentManager` (Alias for `EmergencyCoordinator`)
* `LogisticsManager` (Alias for `ResourceManager`)
* `ShelterAdmin` (Alias for `ShelterAdministrator`)
* `Volunteer` (Operational volunteer)
* `Donor` (Financial contributor)

### Permission-Based Authorization

CareLink employs dynamic **Granular Permission Policies** registered via [Permissions.cs](file:///d:/NSBM\SA-Project\careLink-backend\CareLink.Core\Constants\DefaultRolePermissions.cs) and evaluated by [PermissionAuthorizationHandler.cs](file:///d:/NSBM\SA-Project\careLink-backend\CareLink.API\Authorization\PermissionAuthorizationHandler.cs). Roles are mapped to permissions such as:
* `users.view`, `users.create`, `users.edit`, `users.delete`
* `crisis.create`, `crisis.edit`, `crisis.close`
* `shelters.manage`, `shelters.admit`, `shelters.reports`
* `resources.allocate`, `resources.dispatch`, `resources.procure`
* `ai.view`, `ai.approve`, `ai.execute`
* `audit.view`, `audit.export`

### Administrative Single-Use Setup Tokens

When administrators provision privileged staff accounts (e.g. Case Managers or Shelter Admins):
* Accounts are created with `RequiresPasswordSetup = true` and `AccountStatus = "PendingPasswordSetup"`.
* A single-use, cryptographically generated token with an expiration window (default: 24 hours) is recorded in `SetupToken`.
* Once consumed via `POST /api/auth/setup-password`, the token is invalidated, `RequiresPasswordSetup` is cleared, and `AccountStatus` transitions to `Active`.

---

## 10. API Modules & Endpoints Catalog

CareLink exposes over 60 controllers organized by humanitarian domain:

### Authentication & Identity
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register citizen, volunteer, or partner account | Anonymous |
| `POST` | `/api/auth/login` | Authenticate with email/password and receive JWT | Anonymous |
| `POST` | `/api/auth/setup-password` | Complete password setup using single-use invite token | Anonymous |
| `POST` | `/api/auth/verify-email` | Verify registered user email address | Anonymous |
| `GET` | `/api/auth/profile` | Retrieve profile of the currently signed-in user | JWT Bearer |
| `GET` | `/api/roles` | List all system roles and their assigned permissions | JWT Bearer (`Admin`) |
| `GET` | `/api/permissions` | Retrieve system permissions registry | JWT Bearer (`Admin`) |

### User & Organization Management
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/users` | List platform users with search, role, and district filters | JWT Bearer (`Admin`) |
| `POST` | `/api/users` | Provision internal staff account and send email invitation | JWT Bearer (`Admin`) |
| `PUT` | `/api/users/{id}` | Update user details, role assignment, or status | JWT Bearer (`Admin`) |
| `GET` | `/api/users/stats` | Retrieve user count and distribution statistics | JWT Bearer (`Admin`) |
| `POST` | `/api/users/{id}/resend-invitation` | Resend account setup invitation email | JWT Bearer (`Admin`) |
| `GET` | `/api/organizations` | List registered partner organizations and operational units | JWT Bearer |
| `POST` | `/api/organizations` | Register a new partner organization | JWT Bearer |
| `GET` | `/api/verification/pending` | Retrieve partner organization verification queue | JWT Bearer (`Admin`, `PartnerOfficer`) |
| `POST` | `/api/verification/{id}/decision` | Approve or reject partner organization verification | JWT Bearer (`Admin`) |

### AI & Recommendation Services
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/ai-recommendations` | List AI recommendations with category/state filters | JWT Bearer |
| `GET` | `/api/ai-recommendations/{id}` | Retrieve comprehensive AI recommendation details | JWT Bearer |
| `POST` | `/api/ai-recommendations/{id}/decision`| Approve, reject, or modify an AI recommendation | JWT Bearer (`Admin`, `Coordinator`) |
| `POST` | `/api/ai-recommendations/generate` | Trigger live Gemini AI recommendation generation | JWT Bearer |

### Chatbot Services
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/chatbot/message` | Send prompt to CareLink AI Assistant with grounded context | JWT Bearer |
| `GET` | `/api/chatbot/conversations` | List conversation sessions for the current user | JWT Bearer |
| `GET` | `/api/chatbot/conversations/{id}` | Retrieve message history for a specific conversation | JWT Bearer |

### Emergency & Incident Management
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/crisis-events` | List active crisis incidents (floods, landslides, etc.) | JWT Bearer |
| `POST` | `/api/crisis-events` | Declare a new emergency crisis incident | JWT Bearer (`EmergencyCoordinator`) |
| `GET` | `/api/liveoperationsmap` | Retrieve live geospatial emergency operation markers | JWT Bearer |
| `GET` | `/api/situation-reports` | List operational situation reports (SitReps) | JWT Bearer |
| `GET` | `/api/safety-updates` | Retrieve active public safety and weather broadcasts | Public / JWT Bearer |

### Assistance Requests & Case Management
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/assistance-requests` | List humanitarian requests (food, rescue, shelter, medical) | JWT Bearer |
| `POST` | `/api/assistance-requests` | Create assistance request (administrative intake) | JWT Bearer |
| `GET` | `/api/citizen/requests` | Citizen assistance request submission and tracking | JWT Bearer (`Citizen`) |
| `GET` | `/api/casemanagement/dashboard` | Case manager operational triage dashboard | JWT Bearer (`CaseManager`) |
| `GET` | `/api/mycases` | Cases assigned to the authenticated case worker | JWT Bearer (`CaseManager`) |
| `POST` | `/api/assistance-referrals` | Refer complex cases across partner agencies | JWT Bearer |

### Volunteer Management
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/volunteer/dashboard` | Volunteer coordinator operational metrics | JWT Bearer (`VolunteerCoordinator`) |
| `GET` | `/api/volunteer/directory` | Searchable directory of registered volunteers | JWT Bearer (`VolunteerCoordinator`) |
| `GET` | `/api/volunteer/tasks` | Volunteer field task assignments and tracking | JWT Bearer |
| `POST` | `/api/ai-volunteer-matching` | AI-assisted matching of volunteer skills to open crisis tasks | JWT Bearer (`VolunteerCoordinator`) |
| `GET` | `/api/volunteer-verifications` | Review pending volunteer credential verifications | JWT Bearer (`VolunteerCoordinator`) |

### Shelter & Accommodation Management
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/shelters` | Directory of evacuation centers and relief shelters | Public / JWT Bearer |
| `POST` | `/api/shelters` | Register a new relief shelter or safe zone | JWT Bearer (`ShelterAdministrator`) |
| `GET` | `/api/occupancy` | Real-time shelter occupancy, capacity, and bed counts | JWT Bearer |
| `GET` | `/api/rooms` | Room and bed allocations within designated shelters | JWT Bearer (`ShelterAdministrator`) |
| `GET` | `/api/accommodation-requests` | Triage displaced family shelter intake requests | JWT Bearer |

### Resource & Warehouse Logistics
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/resources` | Catalog of available disaster relief supplies | JWT Bearer |
| `GET` | `/api/warehouses` | Warehouse storage locations and capacity | JWT Bearer (`ResourceManager`) |
| `GET` | `/api/inventory` | Real-time stock counts, expiring rations, and alerts | JWT Bearer (`ResourceManager`) |
| `POST` | `/api/stock-transfers` | Initiate supply movement between warehouses | JWT Bearer (`ResourceManager`) |
| `GET` | `/api/procurement` | Emergency procurement purchase requests | JWT Bearer (`ResourceManager`) |
| `GET` | `/api/dispatches` | Relief transport convoy dispatches and tracking | JWT Bearer (`ResourceManager`) |

### Donations & Financial Tracking
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/donations` | Ledger of financial and in-kind relief donations | JWT Bearer (`FundingOfficer`, `Admin`) |
| `GET` | `/api/donors` | Registered donor directory and contributions | JWT Bearer (`FundingOfficer`) |
| `GET` | `/api/fund-allocations` | Fund distributions assigned to crisis response sectors | JWT Bearer (`FundingOfficer`) |
| `GET` | `/api/expenditures` | Documented field and procurement expenditures | JWT Bearer (`FundingOfficer`) |
| `GET` | `/api/flagged-transactions` | Financial anomaly detection flags requiring review | JWT Bearer (`FundingOfficer`, `Admin`) |
| `GET` | `/api/transparency-reports` | Public and agency financial transparency reports | JWT Bearer |

### Audit Logging & Timeline
| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/audit-logs` | Paginated security and operational audit trail | JWT Bearer (`Admin`) |
| `GET` | `/api/audit-logs/stats` | Audit event statistics by action type and severity | JWT Bearer (`Admin`) |
| `POST` | `/api/audit-logs/export` | Export audit logs to CSV format | JWT Bearer (`Admin`) |
| `GET` | `/api/audit-timeline` | Unified chronological timeline of crisis operations | JWT Bearer |

---

## 11. Database Architecture

### Hybrid Persistence Architecture

CareLink utilizes an optimized **Hybrid Persistence Architecture**:
1. **Persistent Relational Storage (SQL Server):** Used for security-critical, durable domain models requiring referential integrity, ACID guarantees, and audit compliance.
2. **High-Throughput Operational Storage (EF Core In-Memory):** Used for rapid simulation, volatile operational logistics, shift coverages, and demo test scenarios.

```
+-----------------------------------------------------------------------------------------+
|                                    CareLink Backend                                     |
+--------------------------------------------+--------------------------------------------+
                                             |
             +-------------------------------+-------------------------------+
             |                                                               |
             v                                                               v
+-----------------------------------------+     +-----------------------------------------+
|          ApplicationDbContext           |     |            CareLinkDbContext            |
|          [Microsoft SQL Server]         |     |          [EF Core In-Memory]            |
+-----------------------------------------+     +-----------------------------------------+
| * Users (AspNetUsers)                   |     | * Shelters, Rooms, Beds                 |
| * Roles & Permissions                   |     | * AccommodationRequests & Admissions    |
| * Organizations & Partners              |     | * HealthSafetyIncidents & SafetyChecks  |
| * OrganizationDocuments                 |     | * ResourceItems & Distributions         |
| * VerificationRequests                  |     | * Warehouse Stock & Stock Transfers     |
| * AiRecommendations                    |     | * Donations, Donors, & FundAllocations  |
| * ChatbotConversations & Messages       |     | * Expenditures & FlaggedTransactions    |
| * AuditLogs                             |     | * DisasterUpdates & Notifications       |
+-----------------------------------------+     +-----------------------------------------+
```

### ApplicationDbContext (SQL Server)
Configured in [ApplicationDbContext.cs](file:///d:/NSBM/SA-Project/careLink-backend/CareLink.Infrastructure/Data/ApplicationDbContext.cs):
* **`Users`**: Extended `ApplicationUser` containing Full Name, NIC, Address, Phone, District, `RequiresPasswordSetup`, `SetupToken`, `AccountStatus`.
* **`Roles` & `UserRoles`**: Identity roles and user-role relational mappings.
* **`Organizations`**: Registered operational bodies, NGOs, contact details, registration numbers, verification flags.
* **`OrganizationDocuments`**: File metadata for uploaded NGO registration certificates and identity proofs.
* **`AiRecommendations`**: Persistent AI decisions, priority rankings, human reviewer state, and reasoning text.
* **`ChatbotConversations` & `ChatbotMessages`**: Multi-turn conversation sessions between citizens/volunteers and Gemini AI.

### CareLinkDbContext (EF Core In-Memory)
Configured in [CareLinkDbContext.cs](file:///d:/NSBM/SA-Project/careLink-backend/CareLink.Infrastructure/Data/CareLinkDbContext.cs):
* Houses operational tables: `Shelters`, `AccommodationRequests`, `DisasterUpdates`, `ResourceItems`, `Donations`, `Expenditures`, `StaffMembers`, `Rooms`, `Beds`.
* Automatically seeded on startup by [DataSeeder.cs](file:///d:/NSBM/SA-Project/careLink-backend/CareLink.Infrastructure/Data/DataSeeder.cs).

### In-Memory Repositories
Specific coordinator and volunteer tracking endpoints use singleton in-memory repository stores (e.g. `InMemoryAssistanceRequestRepository`, `InMemoryLiveOperationsMapRepository`) for ultra-low latency real-time dispatch updates.

---

## 12. AI Integration (Google Gemini)

### AI Decision-Support Capabilities

CareLink integrates the **Google Gemini API** (`gemini-1.5-flash`) via [GeminiService.cs](file:///d:/NSBM\SA-Project\careLink-backend\CareLink.Infrastructure\Services\GeminiService.cs). The service provides:
* **Assistance Request Triage:** Evaluates citizen requests, assesses vulnerability factors (infants, elderly, flooded residences), and recommends urgency levels (`Critical`, `High`, `Medium`, `Low`).
* **Shelter Placement Suggestions:** Analyzes family sizes, medical mobility constraints, and recommends suitable shelters with available accessible beds.
* **Volunteer Deployment Optimization:** Matches certified skills (first-aid, boat rescue, debris clearance) to incoming incident tasks.
* **Crisis Assistant Chatbot:** Grounded conversational agent assisting affected citizens in finding nearest aid and relief shelters.

### Human-in-the-Loop Review Workflow

> [!IMPORTANT]
> **No AI decision is autonomously executed.** All AI recommendations undergo strict Human-in-the-Loop governance.

1. **AI Generation:** The AI engine analyzes the crisis scenario and produces an `AiRecommendation` marked as `Awaiting Review` / `PendingReview`.
2. **Review Queue:** The recommendation appears on the Emergency Coordinator / Admin dashboard at `GET /api/ai-recommendations`.
3. **Decision & Audit:** The human officer reviews confidence scores, reasoning, and alternatives, then submits an approval or rejection (`POST /api/ai-recommendations/{id}/decision`).
4. **Execution:** Upon approval, the decision is logged to the immutable audit trail and downstream field alerts are triggered.

### Deterministic Fallback Engine

If the Gemini API key is unconfigured, rate-limited, or Google endpoints are temporarily unreachable:
* `GeminiService` detects the condition and logs a warning.
* `AiRecommendationService` automatically invokes a deterministic, rules-based triage algorithm using vulnerability heuristic matrices.
* The application continues functioning without throwing unhandled exceptions.

---

## 13. Email & SMTP Integration

CareLink features a resilient email notification system powered by `System.Net.Mail` via [EmailService.cs](file:///d:/NSBM\SA-Project\careLink-backend\CareLink.Infrastructure\Services\EmailService.cs):

* **Provider:** Gmail SMTP (`smtp.gmail.com`) using port `587` with TLS/STARTTLS.
* **Authentication:** Authenticates using Google 16-character App Passwords (spaces automatically sanitized).
* **Workflows Supported:**
  * **Staff Setup Invitations:** Sends one-time password setup tokens to newly created staff accounts.
  * **Citizen & Volunteer Welcome:** Welcoming newly registered citizens and emergency volunteers.
  * **Partner Verification Updates:** Notifying NGOs regarding pending reviews, approvals, or request for additional documentation.
* **Diagnostic & Fallback Handling:** If SMTP credentials are missing or the network is blocked, `EmailService` captures the failure, categorizes it using `EmailFailureCategories` (`ConfigurationMissing`, `AuthenticationFailure`, `TlsFailure`), and returns user-friendly diagnostics without crashing backend transactions.

---

## 14. Testing

CareLink has a dedicated test project [CareLink.Tests](file:///d:/NSBM\SA-Project\careLink-backend\CareLink.Tests\CareLink.Tests.csproj) built with **xUnit**, **Moq**, and **Microsoft.AspNetCore.Mvc.Testing**.

### Running the Test Suite

Execute the entire backend test suite using the .NET CLI:

```bash
dotnet test CareLink.slnx
```

Or target the test project directly:

```bash
dotnet test CareLink.Tests/CareLink.Tests.csproj
```

### Verified Test Results

The CareLink backend test suite includes **106 automated tests** spanning:
* User registration, password hashing, and login validation
* Administrative user creation and single-use setup token workflows
* Partner organization approval and rejection transitions
* Dynamic permission requirement evaluation (`PermissionAuthorizationHandler`)
* Gmail SMTP failure categorization and configuration diagnostics
* AI chatbot conversation ownership and context grounding
* End-to-end audit logging workflows
* Gemini API service fallbacks using `FakeGeminiService` test doubles

```text
Test run for D:\NSBM\SA-Project\careLink-backend\CareLink.Tests\bin\Debug\net10.0\CareLink.Tests.dll (.NETCoreApp,Version=v10.0)
Passed!  - Failed: 0, Passed: 106, Skipped: 0, Total: 106, Duration: 5 s
```

---

## 15. Frontend Integration

CareLink backend is designed to seamlessly integrate with the **CareLink React Frontend**:

* **Default Frontend Origin:** `http://localhost:5173` (Vite dev server)
* **CORS Policy:** Defined in `Program.cs` under the policy name `"CareLinkFrontend"`. Allows requests from `localhost`, `127.0.0.1`, and `::1` on any port, with credentials, all HTTP methods, and all headers.
* **API Response Envelope:** All application endpoints return a consistent JSON payload structure:
  ```json
  {
    "success": true,
    "data": { ... },
    "message": "Operation completed successfully.",
    "errors": []
  }
  ```
* **Frontend Token Storage:** Upon login, the frontend stores the JWT in `localStorage` or session memory and includes it in the `Authorization: Bearer <token>` header for subsequent API calls.

---

## 16. Known Limitations

During codebase inspection, the following technical characteristics and limitations were identified:

1. **Hybrid Database Model:** Core security entities (Users, Roles, Organizations, Documents, Audit Logs) persist in SQL Server. Operational models (Beds, Rooms, Stock Items) reside in EF Core In-Memory or singleton memory stores. Restarting the backend resets in-memory operational state to the `DataSeeder` baseline.
2. **LocalDB Dependency:** Local development defaults to `(localdb)\mssqllocaldb`. Environments without Windows LocalDB (such as Linux or macOS) must point `ConnectionStrings:DefaultConnection` to an active SQL Server or Docker instance.
3. **Real-time Push:** WebSockets / SignalR hubs are not yet implemented; the frontend polls REST endpoints for status updates.
4. **Document Storage:** Partner verification documents are currently stored on the local disk via `OrganizationDocumentStorageService` rather than cloud blob storage (e.g. AWS S3 or Azure Blob Storage).

---

## 17. Future Improvements

Planned architectural enhancements for upcoming releases:
* **Unified SQL Server Persistence:** Transition all in-memory operational repositories (Shelters, Logistics, Volunteers) to persistent SQL Server tables.
* **Real-time SignalR Hubs:** Implement WebSocket push notifications for immediate disaster dispatch alerts and live chat.
* **Distributed Caching:** Introduce Redis distributed caching for geospatial map markers and live crisis SitReps.
* **Cloud Object Storage:** Integrate Amazon S3 or Azure Blob Storage for encrypted verification documents.
* **Docker Containerization:** Provide a production-ready `docker-compose.yml` orchestrating the ASP.NET Core API and a SQL Server container.

---

## 18. Troubleshooting

### 1. SQL Server Connection Errors
* **Symptom:** `Cannot open database "CareLinkIdentityDb"` or `Server was not found or was not accessible`.
* **Solution:** Verify that SQL Server LocalDB is running:
  ```bash
  sqllocaldb info mssqllocaldb
  sqllocaldb start mssqllocaldb
  ```
  If using SQL Server Express, update `ConnectionStrings:DefaultConnection` in `appsettings.json` to point to `Server=localhost\\SQLEXPRESS;...`.

### 2. Swagger UI Returns 404
* **Symptom:** Navigating to `http://localhost:5277/swagger` returns HTTP 404.
* **Solution:** Confirm that `ASPNETCORE_ENVIRONMENT` is set to `Development` in `CareLink.API/Properties/launchSettings.json`. In .NET 10, Swagger UI requires `Swashbuckle.AspNetCore.SwaggerUI` registered alongside `app.MapOpenApi()`.

### 3. DLL File Lock Errors During Build (`MSB3026` / `MSB3027`)
* **Symptom:** `The process cannot access the file ... CareLink.API.exe because it is being used by another process`.
* **Solution:** Stop any running instance of the API before rebuilding:
  ```powershell
  Stop-Process -Name "CareLink.API" -Force
  dotnet build CareLink.slnx
  ```

### 4. JWT 401 Unauthorized / 403 Forbidden
* **Symptom:** API requests return `401 Unauthorized` or `403 Forbidden`.
* **Solution:** Ensure the `Authorization` header contains `Bearer <TOKEN>` with a space. If 403 occurs, ensure the authenticated account has the required role (e.g., `Admin` or `EmergencyCoordinator`) or permission claim.

### 5. CORS Errors in Frontend Browser Console
* **Symptom:** `Access to fetch at ... from origin 'http://localhost:5173' has been blocked by CORS policy`.
* **Solution:** Ensure `app.UseCors("CareLinkFrontend")` is invoked before `app.UseAuthentication()` and `app.UseAuthorization()` in `Program.cs`. Verify your frontend runs on `localhost` or `127.0.0.1`.

### 6. Gmail SMTP Email Delivery Warnings
* **Symptom:** Account is created but toast indicates invitation email could not be delivered.
* **Solution:** Set a valid 16-character Gmail App Password via .NET User Secrets:
  ```bash
  dotnet user-secrets set "EmailSettings:Password" "xxxx xxxx xxxx xxxx" --project CareLink.API
  ```
  Ensure 2-Step Verification is enabled on your Google account.

---

## License

This project is developed as part of the **CareLink Humanitarian Crisis Management System**. All rights reserved.
