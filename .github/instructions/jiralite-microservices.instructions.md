---
applyTo: "**"
---

# JiraLite Microservices - Hệ thống quản lý công việc nhóm

## Mục tiêu

Refactor JiraLite từ kiến trúc hiện tại sang **Microservices** chuẩn, trong đó mỗi Service là một hệ sinh thái độc lập với **Clean Architecture** riêng và **Database riêng**. Tập trung vào:

- **Service Isolation**: Không có ProjectReference chéo giữa các Service
- **API Gateway (YARP)**: Điểm vào duy nhất từ Client
- **Async Messaging (RabbitMQ)**: Giao tiếp nội bộ giữa các Service
- **Complex Authorization (Policy-based)**: Giữ nguyên logic phân quyền 2 tầng

---

## 1. Kiến trúc tổng quan

### Sơ đồ hệ thống

```
Client (Browser/Mobile)
    │
    ▼
┌─────────────────────────────┐
│   API Gateway (YARP)        │  ← Cổng 5000
│   JiraLite.Gateway          │
│   (Chỉ routing, không có   │
│    nghiệp vụ)               │
└────┬──────────┬─────────────┘
     │          │          │
     ▼          ▼          ▼
┌─────────┐ ┌─────────┐ ┌──────────┐
│Identity │ │Tracking │ │ Logging  │
│Service  │ │Service  │ │ Service  │
│ :5001   │ │ :5002   │ │  :5003   │
│         │ │         │ │          │
│ DB:Auth │ │DB:Main  │ │DB:Log    │
│(Postgres)│ │(Postgres)│ │  (ES)    │
└─────────┘ └────┬────┘ └────▲─────┘
                  │           │
                  └───────────┘
               RabbitMQ (async)
```

### Nguyên tắc cốt lõi

1. **KHÔNG ProjectReference chéo giữa các Service** — Nếu Tracking.API reference Identity.Domain → đó là Monolith
2. **Giao tiếp qua mạng (HTTP) hoặc Message Broker (RabbitMQ)** — Không gọi trực tiếp class/method
3. **Mỗi Service có Database riêng** — Tracking KHÔNG query bảng User, Identity KHÔNG query bảng Issue
4. **SharedKernel CHỈ chứa contracts** — Error classes, Event DTOs, shared enums — KHÔNG chứa Entities hay DbContext
5. **Gateway KHÔNG chứa nghiệp vụ** — Chỉ forward request bằng cấu hình YARP

---

## 2. Cấu trúc Solution (Mono-repo)

```
JiraLite/
│
├── JiraLite.slnx
├── docker-compose.yml
├── docker-compose.override.yml
│
├── src/
│   ├── ApiGateway/
│   │   └── JiraLite.Gateway/
│   │       ├── JiraLite.Gateway.csproj     ← Chỉ YARP config
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── appsettings.Development.json
│   │
│   ├── Services/
│   │   ├── Identity/                        ← IdentityService (Auth + User)
│   │   │   ├── Identity.Domain/
│   │   │   │   ├── Identity.Domain.csproj
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   └── RefreshToken.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   └── SystemRole.cs
│   │   │   │   ├── Errors/                           ← Static error definitions
│   │   │   │   │   ├── UserErrors.cs
│   │   │   │   │   └── AuthErrors.cs
│   │   │   │   └── Interfaces/                       ← Repository contracts
│   │   │   │       ├── IUserRepository.cs
│   │   │   │       ├── IRefreshTokenRepository.cs
│   │   │   │       └── IUnitOfWork.cs
│   │   │   │
│   │   │   ├── Identity.Application/
│   │   │   │   ├── Identity.Application.csproj
│   │   │   │   ├── ApplicationServiceExtensions.cs   ← Đăng ký MediatR
│   │   │   │   ├── Features/                         ← CQRS với MediatR (thay Services/)
│   │   │   │   │   ├── Auth/
│   │   │   │   │   │   ├── Login.cs                  ← Command + Handler
│   │   │   │   │   │   ├── Register.cs
│   │   │   │   │   │   ├── RefreshToken.cs
│   │   │   │   │   │   └── RevokeToken.cs
│   │   │   │   │   └── Users/
│   │   │   │   │       └── GetUserInfo.cs             ← Query + Handler
│   │   │   │   └── DTOs/                             ← HTTP contracts (request/response)
│   │   │   │       ├── LoginRequest.cs
│   │   │   │       ├── LoginResponse.cs
│   │   │   │       ├── RegisterRequest.cs
│   │   │   │       ├── RefreshTokenRequest.cs
│   │   │   │       ├── RefreshTokenResponse.cs
│   │   │   │       └── UserInfoDto.cs
│   │   │   │
│   │   │   ├── Identity.Infrastructure/
│   │   │   │   ├── Identity.Infrastructure.csproj
│   │   │   │   ├── InfrastructureServiceExtensions.cs  ← Đăng ký DbContext, Repositories
│   │   │   │   ├── Data/
│   │   │   │   │   └── AuthDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── UserRepository.cs
│   │   │   │   │   └── RefreshTokenRepository.cs
│   │   │   │   └── Migrations/
│   │   │   │
│   │   │   └── Identity.API/
│   │   │       ├── Identity.API.csproj        ← Chạy cổng 5001
│   │   │       ├── Program.cs
│   │   │       ├── Extensions/
│   │   │       │   └── ApiServiceCollectionExtensions.cs  ← JWT, OpenAPI, Versioning
│   │   │       ├── Apis/
│   │   │       │   ├── IdentityApi.cs         ← Đăng ký tất cả route groups
│   │   │       │   ├── AuthApi.cs
│   │   │       │   └── UsersApi.cs
│   │   │       ├── Filters/
│   │   │       │   └── ApiKeyFilter.cs
│   │   │       └── appsettings.json
│   │   │
│   │   ├── Tracking/                          ← CoreTrackingService (Projects, Issues, Members)
│   │   │   ├── Tracking.Domain/
│   │   │   │   ├── Tracking.Domain.csproj
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Project.cs
│   │   │   │   │   ├── ProjectMember.cs
│   │   │   │   │   └── Issue.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── ProjectRole.cs
│   │   │   │   │   ├── IssueStatus.cs
│   │   │   │   │   └── IssuePriority.cs
│   │   │   │   ├── Errors/                           ← Static error definitions
│   │   │   │   │   ├── ProjectErrors.cs
│   │   │   │   │   └── IssueErrors.cs
│   │   │   │   └── Interfaces/                       ← Repository contracts
│   │   │   │       ├── IProjectRepository.cs
│   │   │   │       ├── IIssueRepository.cs
│   │   │   │       └── IUnitOfWork.cs
│   │   │   │
│   │   │   ├── Tracking.Application/
│   │   │   │   ├── Tracking.Application.csproj
│   │   │   │   ├── ApplicationServiceExtensions.cs   ← Đăng ký MediatR
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IAuditEventPublisher.cs        ← Interface publish event RabbitMQ
│   │   │   │   │   └── IUserService.cs               ← Interface HttpClient gọi IdentityService
│   │   │   │   ├── Features/                         ← CQRS với MediatR
│   │   │   │   │   ├── Projects/
│   │   │   │   │   │   ├── CreateProject.cs
│   │   │   │   │   │   ├── DeactivateProject.cs
│   │   │   │   │   │   ├── GetProjects.cs
│   │   │   │   │   │   └── GetProjectById.cs
│   │   │   │   │   └── Issues/
│   │   │   │   │       ├── CreateIssue.cs
│   │   │   │   │       ├── UpdateIssue.cs
│   │   │   │   │       ├── DeleteIssue.cs
│   │   │   │   │       ├── AssignIssue.cs
│   │   │   │   │       ├── ChangeIssueStatus.cs
│   │   │   │   │       └── GetIssueById.cs
│   │   │   │   └── DTOs/                             ← HTTP contracts
│   │   │   │       ├── Projects/
│   │   │   │       │   ├── CreateProjectRequest.cs
│   │   │   │       │   ├── ProjectDetailDto.cs
│   │   │   │       │   ├── ProjectSummaryDto.cs
│   │   │   │       │   ├── ProjectMemberDto.cs
│   │   │   │       │   └── AddProjectMemberRequest.cs
│   │   │   │       └── Issues/
│   │   │   │           ├── CreateIssueRequest.cs
│   │   │   │           ├── IssueInfoDto.cs
│   │   │   │           ├── IssueDetailDto.cs
│   │   │   │           └── UpdateIssueRequest.cs
│   │   │   │
│   │   │   ├── Tracking.Infrastructure/
│   │   │   │   ├── Tracking.Infrastructure.csproj
│   │   │   │   ├── InfrastructureServiceExtensions.cs  ← Đăng ký DbContext, Repositories
│   │   │   │   ├── Data/
│   │   │   │   │   ├── TrackingDbContext.cs
│   │   │   │   │   └── UnitOfWork.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── ProjectRepository.cs
│   │   │   │   │   └── IssueRepository.cs
│   │   │   │   └── Migrations/
│   │   │   │
│   │   │   └── Tracking.API/
│   │   │       ├── Tracking.API.csproj            ← Chạy cổng 5002
│   │   │       ├── Program.cs
│   │   │       ├── Extensions/
│   │   │       │   ├── ApiServiceCollectionExtensions.cs  ← JWT, OpenAPI, Versioning
│   │   │       │   └── ClaimsPrincipalExtensions.cs
│   │   │       ├── Apis/
│   │   │       │   ├── TrackingApi.cs             ← Đăng ký tất cả route groups
│   │   │       │   ├── ProjectsApi.cs
│   │   │       │   ├── ProjectMembersApi.cs
│   │   │       │   ├── ProjectIssuesApi.cs
│   │   │       │   └── IssuesApi.cs
│   │   │       ├── Authorization/
│   │   │       │   ├── Constants/
│   │   │       │   │   └── PolicyNames.cs
│   │   │       │   ├── Requirements/
│   │   │       │   │   ├── ProjectMemberRequirement.cs
│   │   │       │   │   ├── ProjectManagerRequirement.cs
│   │   │       │   │   ├── AdminOrProjectMemberRequirement.cs
│   │   │       │   │   ├── AdminOrProjectManagerRequirement.cs
│   │   │       │   │   └── ProjectManagerOrAssigneeRequirement.cs
│   │   │       │   ├── Handlers/
│   │   │       │   │   └── ProjectAuthorizationHandler.cs
│   │   │       │   └── Extensions/
│   │   │       │       └── AuthorizationExtensions.cs
│   │   │       └── appsettings.json
│   │   │
│   │   └── Logging/                               ← LoggingService (Audit trail)
│   │       ├── Logging.Domain/
│   │       │   ├── Logging.Domain.csproj
│   │       │   ├── Entities/
│   │       │   │   └── AuditLog.cs
│   │       │   ├── Errors/                           ← Static error definitions
│   │       │   │   └── LogErrors.cs
│   │       │   └── Interfaces/                       ← Repository contracts
│   │       │       └── IAuditLogRepository.cs
│   │       │
│   │       ├── Logging.Application/
│   │       │   ├── Logging.Application.csproj
│   │       │   ├── ApplicationServiceExtensions.cs   ← Đăng ký MediatR
│   │       │   ├── Features/                         ← CQRS với MediatR (query handlers)
│   │       │   │   └── Logs/
│   │       │   │       ├── GetAuditLogs.cs
│   │       │   │       └── GetAuditLogsByIssue.cs
│   │       │   └── Consumers/                        ← RabbitMQ consumers
│   │       │       ├── IssueCreatedConsumer.cs
│   │       │       ├── IssueStatusChangedConsumer.cs
│   │       │       ├── IssueAssignedConsumer.cs
│   │       │       └── IssuePriorityChangedConsumer.cs
│   │       │
│   │       ├── Logging.Infrastructure/
│   │       │   ├── Logging.Infrastructure.csproj
│   │       │   ├── InfrastructureServiceExtensions.cs  ← Đăng ký Elasticsearch, Repositories
│   │       │   ├── Elasticsearch/
│   │       │   │   └── ElasticsearchClientFactory.cs
│   │       │   └── Repositories/
│   │       │       └── AuditLogRepository.cs
│   │       │
│   │       └── Logging.API/
│   │           ├── Logging.API.csproj             ← Chạy cổng 5003
│   │           ├── Program.cs
│   │           ├── Extensions/
│   │           │   └── ApiServiceCollectionExtensions.cs  ← OpenAPI, Versioning
│   │           ├── Apis/
│   │           │   ├── LoggingApi.cs              ← Đăng ký tất cả route groups
│   │           │   └── LogsApi.cs
│   │           └── appsettings.json
│   │
│   └── SharedKernel/
│       ├── JiraLite.Shared.Contracts/
│       │   ├── JiraLite.Shared.Contracts.csproj
│       │   ├── Common/
│       │   │   ├── Result.cs
│       │   │   ├── Error.cs
│       │   │   ├── PaginationRequest.cs
│       │   │   └── PaginationResponse.cs
│       │   └── Settings/
│       │       └── JwtSettings.cs
│       │
│       └── JiraLite.Shared.Messaging/
│           ├── JiraLite.Shared.Messaging.csproj
│           └── Events/
│               ├── IssueCreatedEvent.cs
│               ├── IssueStatusChangedEvent.cs
│               ├── IssueAssignedEvent.cs
│               └── IssuePriorityChangedEvent.cs
│
└── tests/
    ├── Identity.UnitTests/
    ├── Tracking.UnitTests/
    ├── Tracking.IntegrationTests/
    └── Gateway.IntegrationTests/
```

---

## 3. Phân quyền 2 tầng (Giữ nguyên logic)

### Tầng 1: SystemRole (Toàn hệ thống — Nằm trong JWT)

| Role      | Quyền hạn                                                          |
| --------- | ------------------------------------------------------------------ |
| **Admin** | Tạo Project, Phân công User vào Project, Gán role (Manager/Member) |
| **User**  | Chỉ làm việc trong Project được phân công                          |

### Tầng 2: ProjectRole (Trong từng Project — Query DB runtime)

| Role        | Quyền hạn                                                                            |
| ----------- | ------------------------------------------------------------------------------------ |
| **Manager** | Tạo Issue, Assign Issue cho Member, Xóa Issue, Xem tất cả Issues                     |
| **Member**  | Xem Issue được assign, Update trạng thái (Todo → InProgress → Done), Không xóa Issue |

### Nơi chứa Authorization logic

Authorization nằm **bên trong Tracking.API** (không tách thành project riêng), vì:

- Authorization handler cần truy cập `TrackingDbContext` để query `ProjectMembers`
- Chỉ Tracking Service cần logic phân quyền Project/Issue
- Identity Service có phân quyền riêng (API Key filter cho internal endpoints)

---

## 4. JWT Authentication Design

### JWT Token chỉ chứa SystemRole

```json
{
    "sub": "user-guid",
    "email": "user@example.com",
    "name": "John Doe",
    "role": "User",
    "jti": "unique-token-id",
    "iat": 1707200000,
    "exp": 1707203600,
    "iss": "JiraLite.Auth.Api",
    "aud": "JiraLite Client"
}
```

### Token Validation

- **Identity.API** phát hành JWT (login, refresh)
- **Tracking.API** validate JWT bằng shared `JwtSettings` (cùng SecretKey, Issuer, Audience)
- **Gateway** KHÔNG validate JWT — chỉ forward header `Authorization: Bearer xxx`
- **Logging.API** không cần JWT — chỉ consume events từ RabbitMQ + có thể expose read-only API với API Key

---

## 5. Giao tiếp giữa các Service

### 5.1. Synchronous (HTTP) — Chỉ khi cần response ngay

| Caller       | Callee       | Mục đích                         | Cách thực hiện                |
| ------------ | ------------ | -------------------------------- | ----------------------------- |
| Tracking.API | Identity.API | Lấy thông tin User khi AddMember | `HttpClient` + API Key header |
| Gateway      | Identity.API | Forward auth requests            | YARP reverse proxy            |
| Gateway      | Tracking.API | Forward project/issue requests   | YARP reverse proxy            |

**Tracking.API gọi Identity.API:**

```csharp
// Trong Tracking.Application/Interfaces/IUserService.cs + triển khai trong Infrastructure
public class UserService(HttpClient httpClient) : IUserService
{
    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync($"/api/internal/users/{userId}", ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserInfoDto>(ct);
    }
}
```

### 5.2. Asynchronous (RabbitMQ) — Fire-and-forget

| Publisher    | Event                       | Consumer    | Mục đích      |
| ------------ | --------------------------- | ----------- | ------------- |
| Tracking.API | `IssueCreatedEvent`         | Logging.API | Ghi audit log |
| Tracking.API | `IssueStatusChangedEvent`   | Logging.API | Ghi audit log |
| Tracking.API | `IssueAssignedEvent`        | Logging.API | Ghi audit log |
| Tracking.API | `IssuePriorityChangedEvent` | Logging.API | Ghi audit log |

**Sử dụng MassTransit + RabbitMQ:**

```csharp
// Tracking publish event
await publishEndpoint.Publish(new IssueCreatedEvent
{
    IssueId = issue.Id,
    ProjectId = issue.ProjectId,
    Title = issue.Title,
    CreatedById = currentUserId,
    CreatedAt = DateTime.UtcNow
}, cancellationToken);

// Logging consume event
public class IssueCreatedConsumer(IAuditLogRepository repo) : IConsumer<IssueCreatedEvent>
{
    public async Task Consume(ConsumeContext<IssueCreatedEvent> context)
    {
        var log = new AuditLog { ... };
        await repo.AddAsync(log, context.CancellationToken);
    }
}
```

---

## 6. API Gateway (YARP)

### Route Configuration

```json
{
    "ReverseProxy": {
        "Routes": {
            "identity-route": {
                "ClusterId": "identity-cluster",
                "Match": { "Path": "/api/auth/{**catch-all}" }
            },
            "identity-users-route": {
                "ClusterId": "identity-cluster",
                "Match": { "Path": "/api/v1/users/{**catch-all}" }
            },
            "tracking-projects-route": {
                "ClusterId": "tracking-cluster",
                "Match": { "Path": "/api/v1/projects/{**catch-all}" }
            },
            "tracking-issues-route": {
                "ClusterId": "tracking-cluster",
                "Match": { "Path": "/api/v1/issues/{**catch-all}" }
            },
            "logging-route": {
                "ClusterId": "logging-cluster",
                "Match": { "Path": "/api/v1/logs/{**catch-all}" }
            }
        },
        "Clusters": {
            "identity-cluster": {
                "Destinations": {
                    "destination1": { "Address": "http://localhost:5001/" }
                }
            },
            "tracking-cluster": {
                "Destinations": {
                    "destination1": { "Address": "http://localhost:5002/" }
                }
            },
            "logging-cluster": {
                "Destinations": {
                    "destination1": { "Address": "http://localhost:5003/" }
                }
            }
        }
    }
}
```

### Gateway Program.cs (Minimal)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();
```

---

## 7. API Endpoints Design

### Identity.API (Cổng 5001)

```
# Public endpoints (qua Gateway)
POST /api/auth/register              # Đăng ký
POST /api/auth/login                 # Đăng nhập → JWT
POST /api/auth/refresh-token         # Refresh access token
POST /api/auth/revoke                # Logout (revoke refresh token)

# Internal endpoints (chỉ service-to-service, bảo vệ bằng API Key)
GET  /api/internal/users/{userId}    # Lấy thông tin user (Tracking gọi)
GET  /api/internal/users?email=x     # Tìm user bằng email
GET  /api/internal/users             # Danh sách users (phân trang)
```

### Tracking.API (Cổng 5002)

```
# Projects
GET    /api/v1/projects                              # List projects của user
POST   /api/v1/projects                              # Tạo project [Admin]
GET    /api/v1/projects/{projectId}                  # Chi tiết [Member/Manager]
PUT    /api/v1/projects/{projectId}                  # Cập nhật [Admin/Manager]
DELETE /api/v1/projects/{projectId}                  # Xóa [Admin]

# Project Members
GET    /api/v1/projects/{projectId}/members          # List members [Member/Manager]
POST   /api/v1/projects/{projectId}/members          # Add member [Admin]
PUT    /api/v1/projects/{projectId}/members/{userId} # Update role [Admin/Manager]
DELETE /api/v1/projects/{projectId}/members/{userId} # Remove member [Admin/Manager]

# Project Issues
GET    /api/v1/projects/{projectId}/issues           # List issues [Manager: all, Member: assigned only]
POST   /api/v1/projects/{projectId}/issues           # Tạo issue [Manager]

# Issues
GET    /api/v1/issues/{issueId}                      # Chi tiết issue
PUT    /api/v1/issues/{issueId}                      # Update issue [Manager]
PUT    /api/v1/issues/{issueId}/assign               # Assign [Manager]
PUT    /api/v1/issues/{issueId}/status               # Update status [Assignee/Manager]
PUT    /api/v1/issues/{issueId}/priority              # Update priority [Manager]
DELETE /api/v1/issues/{issueId}                      # Xóa [Manager]
GET    /api/v1/issues/my-issues                      # Dashboard cá nhân
```

### Logging.API (Cổng 5003)

```
# Read-only API (bảo vệ bằng JWT hoặc API Key)
GET /api/v1/logs                                     # Query audit logs (filter, phân trang)
GET /api/v1/logs/issues/{issueId}                    # Logs theo issue
GET /api/v1/logs/projects/{projectId}                # Logs theo project

# Không có POST — dữ liệu đến qua RabbitMQ consumers
```

---

## 8. Project Reference Rules

### Allowed References (Dependency Flow)

```
Trong mỗi Service, dependency chạy từ ngoài vào trong (Clean Architecture):

API → Application → Domain
API → Infrastructure → Domain
(Infrastructure KHÔNG reference Application)

Cụ thể:
├── Identity.API           → Identity.Application, Identity.Infrastructure
├── Identity.Application   → Identity.Domain
├── Identity.Infrastructure→ Identity.Domain
│
├── Tracking.API           → Tracking.Application, Tracking.Infrastructure
├── Tracking.Application   → Tracking.Domain, JiraLite.Shared.Messaging
├── Tracking.Infrastructure→ Tracking.Domain
│
├── Logging.API            → Logging.Application, Logging.Infrastructure
├── Logging.Application    → Logging.Domain, JiraLite.Shared.Messaging
├── Logging.Infrastructure → Logging.Domain
│
├── JiraLite.Gateway       → (KHÔNG reference bất kỳ service nào)
│
└── SharedKernel:
    ├── JiraLite.Shared.Contracts  ← Mọi Application project đều có thể reference
    └── JiraLite.Shared.Messaging  ← Tracking.Application + Logging.Application reference
```

### FORBIDDEN References (Vi phạm = Monolith)

```
❌ Tracking.* → Identity.*          (Gọi qua HTTP thay vì reference)
❌ Identity.* → Tracking.*          (Tuyệt đối không)
❌ Logging.*  → Tracking.*          (Consume events từ RabbitMQ)
❌ Logging.*  → Identity.*          (Không cần)
❌ Gateway    → Bất kỳ Service nào  (Chỉ forward bằng YARP config)
❌ *.Infrastructure → *.Application  (Vi phạm Clean Architecture)
```

---

## 9. Database Design

### Database: JiraLiteAuthDb (Identity Service - PostgreSQL)

```
┌─────────────────────┐     ┌──────────────────────┐
│ Users               │     │ RefreshTokens         │
├─────────────────────┤     ├──────────────────────┤
│ Id (PK, Guid)       │◄───┤│ UserId (FK)          │
│ Email               │     │ Id (PK, Guid)         │
│ PasswordHash        │     │ TokenHash             │
│ FullName            │     │ ExpiresAt             │
│ Role (SystemRole)   │     │ CreatedAt             │
│ IsActive            │     │ RevokedAt             │
│ CreatedAt           │     └──────────────────────┘
└─────────────────────┘
```

### Database: JiraLiteTrackingDb (Tracking Service - PostgreSQL)

```
┌───────────────────┐     ┌─────────────────────┐     ┌──────────────────────┐
│ Projects          │     │ ProjectMembers      │     │ Issues               │
├───────────────────┤     ├─────────────────────┤     ├──────────────────────┤
│ Id (PK, Guid)     │◄───┤│ ProjectId (FK)      │     │ Id (PK, Guid)        │
│ Name              │     │ Id (PK, Guid)        │◄───┤│ AssignedToId (FK)    │
│ Description       │     │ UserId (Guid)        │     │ ProjectId (FK)       │→ Projects
│ CreatedAt         │     │ FullName             │     │ Title                │
│ UpdatedAt         │     │ Email                │     │ Description          │
│ IsActive          │     │ Role (ProjectRole)   │     │ Status (IssueStatus) │
└───────────────────┘     │ IsActive             │     │ Priority             │
                          │ JoinedAt             │     │ CreatedAt            │
                          └─────────────────────┘     │ UpdatedAt            │
                                                       └──────────────────────┘

Lưu ý: ProjectMembers.UserId là Guid tham chiếu logic đến Users.Id
bên Identity, KHÔNG có FK constraint vật lý (cross-database).
ProjectMembers lưu snapshot FullName + Email tại thời điểm add.
```

### Index: audit-logs (Logging Service - Elasticsearch 8.x)

Elasticsearch lưu AuditLog dưới dạng JSON document trong index `audit-logs`:

```json
{
    "id": "guid",
    "eventType": "IssueCreated",
    "entityType": "Issue",
    "entityId": "guid",
    "projectId": "guid",
    "userId": "guid",
    "oldValue": "string | null",
    "newValue": "string | null",
    "description": "string | null",
    "timestamp": "2026-03-02T00:00:00Z"
}
```

Lợi ích so với PostgreSQL:

- Append-only pattern → phù hợp hoàn toàn với audit log (không update/delete)
- Full-text search trên `description`, `eventType` nhanh
- Filter đa chiều: `projectId` + `userId` + date range mà không cần index phức tạp
- Không cần EF Core migrations — schema tự động từ document
- Default username: `elastic`, password qua `ELASTIC_PASSWORD` trong `.env`

---

## 10. Domain Entities (Per Service)

### Identity.Domain

```csharp
// Entities/User.cs
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public SystemRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

// Entities/RefreshToken.cs
public class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}

// Enums/SystemRole.cs
public enum SystemRole { User = 0, Admin = 1 }
```

### Tracking.Domain

```csharp
// Entities/Project.cs
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<Issue> Issues { get; set; } = [];
}

// Entities/ProjectMember.cs — Lưu snapshot user info
public class ProjectMember
{
    public Guid Id { get; set; }
    public ProjectRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string FullName { get; set; } = string.Empty;  // Snapshot từ Identity Service
    public string Email { get; set; } = string.Empty;      // Snapshot từ Identity Service
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }   // Logical reference → Identity.User.Id
    public Project Project { get; set; } = default!;
    public ICollection<Issue> AssignedIssues { get; set; } = [];
}

// Entities/Issue.cs
public class Issue
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.ToDo;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Project Project { get; set; } = null!;
    public ProjectMember? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// Enums (nội bộ Tracking, KHÔNG nằm trong SharedKernel)
public enum ProjectRole { Member = 0, Manager = 1 }
public enum IssueStatus { ToDo = 0, InProgress = 1, Done = 2, Rejected = 3 }
public enum IssuePriority { Low = 0, Medium = 1, High = 2, Critical = 3 }
public enum IssueChangeType { Created = 0, StatusChanged = 1, AssigneeChanged = 2, PriorityChanged = 3, TitleChanged = 4, DescriptionChanged = 5, Rejected = 6 }
```

### Logging.Domain

```csharp
// Entities/AuditLog.cs
public class AuditLog
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

---

## 11. Shared Messaging Events

```csharp
// JiraLite.Shared.Messaging/Events/IssueCreatedEvent.cs
public record IssueCreatedEvent
{
    public required Guid IssueId { get; init; }
    public required Guid ProjectId { get; init; }
    public required string Title { get; init; }
    public required string Priority { get; init; }
    public required Guid CreatedById { get; init; }
    public required DateTime CreatedAt { get; init; }
}

// IssueStatusChangedEvent.cs
public record IssueStatusChangedEvent
{
    public required Guid IssueId { get; init; }
    public required Guid ProjectId { get; init; }
    public required Guid ChangedById { get; init; }
    public required string OldStatus { get; init; }
    public required string NewStatus { get; init; }
    public required DateTime ChangedAt { get; init; }
}

// IssueAssignedEvent.cs
public record IssueAssignedEvent
{
    public required Guid IssueId { get; init; }
    public required Guid ProjectId { get; init; }
    public required Guid AssignedById { get; init; }
    public required Guid AssignedToId { get; init; }
    public Guid? PreviousAssigneeId { get; init; }
    public required DateTime AssignedAt { get; init; }
}

// IssuePriorityChangedEvent.cs
public record IssuePriorityChangedEvent
{
    public required Guid IssueId { get; init; }
    public required Guid ProjectId { get; init; }
    public required Guid ChangedById { get; init; }
    public required string OldPriority { get; init; }
    public required string NewPriority { get; init; }
    public required DateTime ChangedAt { get; init; }
}
```

> **Lưu ý:** Events dùng `string` cho Status/Priority thay vì enum, để tránh SharedKernel phụ thuộc vào Tracking.Domain enums.

---

## 12. docker-compose.yml

```yaml
services:
    # ─────────────────────────────────────────
    # Identity Service Database (PostgreSQL)
    # ─────────────────────────────────────────
    auth-db:
        image: postgres:16-alpine
        container_name: jiralite-auth-db
        restart: always
        environment:
            POSTGRES_USER: ${POSTGRES_USER}
            POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
            POSTGRES_DB: JiraLiteAuthDb
        ports:
            - "5432:5432"
        volumes:
            - auth_postgres_data:/var/lib/postgresql/data

    # ─────────────────────────────────────────
    # Tracking Service Database (PostgreSQL)
    # ─────────────────────────────────────────
    tracking-db:
        image: postgres:16-alpine
        container_name: jiralite-tracking-db
        restart: always
        environment:
            POSTGRES_USER: ${POSTGRES_USER}
            POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
            POSTGRES_DB: JiraLiteTrackingDb
        ports:
            - "5433:5432"
        volumes:
            - tracking_postgres_data:/var/lib/postgresql/data

    # ─────────────────────────────────────────
    # Logging Service Database (Elasticsearch)
    # ─────────────────────────────────────────
    elasticsearch:
        image: docker.elastic.co/elasticsearch/elasticsearch:8.13.0
        container_name: jiralite-elasticsearch
        restart: always
        environment:
            - discovery.type=single-node
            - ELASTIC_PASSWORD=${ELASTIC_PASSWORD}
            - xpack.security.enabled=true
            - xpack.security.http.ssl.enabled=false
            - ES_JAVA_OPTS=-Xms512m -Xmx512m
        ports:
            - "9200:9200"
        volumes:
            - elasticsearch_data:/usr/share/elasticsearch/data

    # ─────────────────────────────────────────
    # Message Broker (RabbitMQ)
    # ─────────────────────────────────────────
    rabbitmq:
        image: rabbitmq:3-management-alpine
        container_name: jiralite-rabbitmq
        restart: always
        environment:
            RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
            RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
        ports:
            - "5672:5672" # AMQP
            - "15672:15672" # Management UI
        volumes:
            - rabbitmq_data:/var/lib/rabbitmq

volumes:
    auth_postgres_data:
    tracking_postgres_data:
    elasticsearch_data:
    rabbitmq_data:
```

---

## 13. Authorization Policies (Trong Tracking.API)

```csharp
public static class PolicyNames
{
    // System-level (JWT claim "role")
    public const string RequireAdmin = "RequireAdminRole";

    // Project-level (DB query tại runtime)
    public const string ProjectMember = "ProjectMember";
    public const string ProjectManager = "ProjectManager";
    public const string AdminOrProjectMember = "AdminOrProjectMember";
    public const string AdminOrProjectManager = "AdminOrProjectManager";

    // Issue-level
    public const string ProjectManagerOrAssignee = "ProjectManagerOrAssignee";
}
```

---

## 14. Key NuGet Packages (Per Service)

### Gateway

- `Yarp.ReverseProxy`

### Identity Service

- `MediatR` (CQRS)
- `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.AspNetCore.Identity` (PasswordHasher)
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Asp.Versioning.Http` (API Versioning)

### Tracking Service

- `MediatR` (CQRS)
- `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `MassTransit.RabbitMQ` (Publish events)
- `Asp.Versioning.Http` (API Versioning)

### Logging Service

- `MediatR` (CQRS — cho query handlers)
- `Elastic.Clients.Elasticsearch` (Elasticsearch 8.x client)
- `MassTransit.RabbitMQ` (Consume events)
- `Asp.Versioning.Http` (API Versioning)

### SharedKernel

- Không có NuGet ngoài — chỉ pure C# DTOs/records

---

## 15. Tiến độ triển khai (Refactoring Phases)

### Phase 1: Restructure Solution ⬜

- [ ] Tạo thư mục `src/` với cấu trúc mới
- [ ] Tạo `src/ApiGateway/JiraLite.Gateway` project
- [ ] Di chuyển Identity entities/services vào `src/Services/Identity/`
- [ ] Di chuyển Tracking entities/services vào `src/Services/Tracking/`
- [ ] Tạo `src/Services/Logging/` projects
- [ ] Tạo `src/SharedKernel/` projects
- [ ] Cập nhật `JiraLite.slnx`
- [ ] Xóa các project cũ (JiraLite.Api, JiraLite.Auth.Api, JiraLite.Authorization, etc.)

### Phase 2: Identity Service ⬜

- [ ] Identity.Domain — Entities + Enums
- [ ] Identity.Infrastructure — AuthDbContext + Migrations
- [ ] Identity.Application — Services (Auth, JWT, User)
- [ ] Identity.API — Endpoints (Auth public + Internal)
- [ ] API Key filter cho internal endpoints

### Phase 3: Tracking Service ⬜

- [ ] Tracking.Domain — Entities + Enums
- [ ] Tracking.Infrastructure — TrackingDbContext + Migrations
- [ ] Tracking.Application — Services + HttpClient UserService
- [ ] Tracking.API — Endpoints + Authorization (Policies, Requirements, Handlers)
- [ ] JWT validation (shared JwtSettings config)

### Phase 4: API Gateway (YARP) ⬜

- [ ] Cài đặt YARP
- [ ] Cấu hình routes → Identity/Tracking/Logging clusters
- [ ] Test end-to-end qua Gateway

### Phase 5: Messaging (RabbitMQ) ⬜

- [ ] Thêm RabbitMQ vào docker-compose
- [ ] SharedKernel.Messaging — Event DTOs
- [ ] Tracking.Application — Publish events (MassTransit)
- [ ] Logging.Application — Consumers

### Phase 6: Logging Service ⬜

- [ ] Logging.Domain — AuditLog entity
- [ ] Logging.Infrastructure — LoggingDbContext + Migrations
- [ ] Logging.Application — Consumers + AuditLogService
- [ ] Logging.API — Read-only query endpoints

### Phase 7: Docker & Testing ⬜

- [ ] Dockerfile cho mỗi Service
- [ ] docker-compose cho full stack (3 DBs + RabbitMQ + 4 Services)
- [ ] Integration tests cho Authorization
- [ ] End-to-end tests qua Gateway

---

## 16. Mapping: Cũ → Mới

| Project cũ                               | Đích mới                                                         |
| ---------------------------------------- | ---------------------------------------------------------------- |
| `JiraLite.Auth.Api`                      | `src/Services/Identity/Identity.API`                             |
| `JiraLite.Auth.Infrastructure`           | `src/Services/Identity/Identity.Infrastructure`                  |
| Auth entities (User, RefreshToken)       | `src/Services/Identity/Identity.Domain/Entities`                 |
| `JiraLite.Api`                           | `src/Services/Tracking/Tracking.API`                             |
| `JiraLite.Application`                   | `src/Services/Tracking/Tracking.Application`                     |
| `JiraLite.Infrastructure`                | `src/Services/Tracking/Tracking.Infrastructure`                  |
| Tracking entities (Project, Issue, etc.) | `src/Services/Tracking/Tracking.Domain/Entities`                 |
| `JiraLite.Authorization`                 | `src/Services/Tracking/Tracking.API/Authorization/`              |
| `JiraLite.Share/Common`                  | `src/SharedKernel/JiraLite.Shared.Contracts/Common`              |
| `JiraLite.Share/Settings`                | `src/SharedKernel/JiraLite.Shared.Contracts/Settings`            |
| `JiraLite.Share/Enums`                   | Phân tán vào Domain của từng Service                             |
| `JiraLite.Share/Dtos`                    | Phân tán vào Application/DTOs của từng Service                   |
| `DbLogService` (ILogService)             | Thay bằng RabbitMQ publish + Logging Service consume             |
| `IssueChangeLog` entity                  | **Xóa** khỏi Tracking DB → thay bằng `AuditLog` trong Logging DB |

---

## 17. Coding Guidelines

### Naming Conventions

- **Entities**: `User`, `Project`, `Issue` (không dùng `Task`)
- **DTOs**: `CreateProjectRequest`, `ProjectResponse`
- **Services**: `I{Name}Service` / `{Name}Service`
- **Events**: `{Entity}{Action}Event` — `IssueCreatedEvent`, `IssueStatusChangedEvent`
- **Consumers**: `{EventName}Consumer` — `IssueCreatedConsumer`
- **Namespaces**: `{ServiceName}.{Layer}` — `Identity.Domain.Entities`, `Tracking.Application.Services`

### Best Practices

- JWT chỉ chứa SystemRole, ProjectRole query từ DB tại runtime
- Dùng `required` và `init` cho DTOs và Events (immutable)
- Dùng `CancellationToken` cho tất cả async methods
- Dùng User Secrets cho sensitive data (SecretKey, API Key, Connection Strings)
- Dùng `IHttpClientFactory` cho service-to-service HTTP calls
- Dùng `AsNoTracking()` cho read-only queries
- Events dùng `string` thay vì enum để tránh coupling SharedKernel ↔ Domain
- Mỗi Service chạy trên process riêng, cổng riêng
- IssueChangeLog **không còn** là bảng trong Tracking DB — audit trail xử lý bởi Logging Service (Elasticsearch)

### Configuration (appsettings.json mẫu)

**Identity.API:**

```json
{
    "ConnectionStrings": { "PostgreSqlConnection": "" },
    "JwtSettings": {
        "SecretKey": "",
        "Issuer": "JiraLite.Auth.Api",
        "Audience": "JiraLite Client",
        "AccessTokenExpiryInMinutes": 15,
        "RefreshTokenExpiryInDays": 7
    },
    "ApiKeys": { "InternalApiKey": "" }
}
```

**Tracking.API:**

```json
{
    "ConnectionStrings": { "PostgreSqlConnection": "" },
    "JwtSettings": {
        "SecretKey": "",
        "Issuer": "JiraLite.Auth.Api",
        "Audience": "JiraLite Client"
    },
    "IdentityApi": { "BaseUrl": "http://localhost:5001", "ApiKey": "" },
    "RabbitMQ": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest"
    }
}
```

**Logging.API:**

```json
{
    "Elasticsearch": {
        "Uri": "http://localhost:9200",
        "Username": "elastic",
        "Password": "",
        "IndexName": "audit-logs"
    },
    "RabbitMQ": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest"
    }
}
```

**Gateway:**

```json
{
    "ReverseProxy": { "Routes": { "..." }, "Clusters": { "..." } }
}
```
