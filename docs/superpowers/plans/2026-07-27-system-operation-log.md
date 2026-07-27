# System Operation Log Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a generic, low-touch operation log that automatically records entity changes with inferred `ActionType` and computed `ChangeJson`.

**Architecture:** Add `SystemOperationLog` as a system domain entity and expose it through `IDbContext`. Extend current user/request context so `AppDbContext` can collect request metadata, then centralize operation-log collection inside `SaveChanges` and `SaveChangesAsync`. Add a simple query endpoint for audit list and entity timeline usage.

**Tech Stack:** .NET 8, C#, EF Core 8, ASP.NET Core, MediatR, SQL Server provider, xUnit/EF InMemory verification in `CodexTemp`.

## Global Constraints

- 所有回答和面向用户说明使用中文。
- 不要把中间产物放在代码仓库里。
- 临时文件、草稿、分析结果、构建输出、实验脚本或调试产物统一放到 `CodexTemp`。
- 前端和后端相关的测试文件统一放到 `CodexTemp`，不要直接放进代码仓库。
- 如果必须修改仓库内文件，只修改必要的源代码或配置文件，并说明修改内容。
- 文本文件保持 UTF-8 编码。
- 操作日志不保存 `ModuleName`、`ActionName`、`Summary`、`ChangedFields`、`BeforeJson`、`AfterJson`、`RequestMethod`。
- `ActionType` 和 `ChangeJson` 必须由系统自动推断/计算，业务 Command/Service 不显式指定。
- 失败日志继续交给现有 `SystemErrorLog`，操作日志只记录成功保存的业务变更。

---

## File Structure

- Create `src/1.QPS.Domain/Entities/System/SystemOperationLog.cs`: operation log aggregate/entity.
- Modify `src/2.QPS.Application/Interfaces/ICurrentUserService.cs`: expose request path, IP, and User-Agent.
- Modify `src/3.QPS.Infrastructure/Identity/CurrentUserService.cs`: read request metadata from `IHttpContextAccessor`.
- Modify `src/2.QPS.Application/Interfaces/IDbContext.cs`: add `DbSet<SystemOperationLog>`.
- Modify `src/3.QPS.Infrastructure/Database/AppDbContext.cs`: add `DbSet`, configure indexes/lengths, collect logs in save pipeline.
- Create `src/2.QPS.Application/Contracts/System/OperationLogs/OperationLogDto.cs`: API DTO.
- Create `src/2.QPS.Application/Contracts/System/OperationLogs/OperationLogQueryRequest.cs`: query request.
- Create `src/2.QPS.Application/Features/System/OperationLogs/GetOperationLogsQuery.cs`: MediatR query.
- Create `src/4.QPS.WebAPI/Controllers/Admin/System/OperationLogController.cs`: list endpoint.
- Create verification project/files under `CodexTemp/SystemOperationLogChecks`: executable smoke checks only, not committed.

---

### Task 1: Domain Entity And Context Surface

**Files:**
- Create: `src/1.QPS.Domain/Entities/System/SystemOperationLog.cs`
- Modify: `src/2.QPS.Application/Interfaces/IDbContext.cs`
- Modify: `src/3.QPS.Infrastructure/Database/AppDbContext.cs`

**Interfaces:**
- Produces: `SystemOperationLog.Create(...)`
- Produces: `IDbContext.SystemOperationLogs`
- Produces: `AppDbContext.SystemOperationLogs`

- [ ] **Step 1: Create the domain entity**

Add `src/1.QPS.Domain/Entities/System/SystemOperationLog.cs`:

```csharp
using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemOperationLog : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public string ChangeJson { get; private set; } = string.Empty;
    public string OperatorUserId { get; private set; } = string.Empty;
    public string OperatorName { get; private set; } = string.Empty;
    public string RequestPath { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;

    private SystemOperationLog() { }

    private SystemOperationLog(
        string entityType,
        string entityId,
        string actionType,
        string changeJson,
        string operatorUserId,
        string operatorName,
        string requestPath,
        string ipAddress,
        string userAgent)
    {
        EntityType = entityType;
        EntityId = entityId;
        ActionType = actionType;
        ChangeJson = changeJson;
        OperatorUserId = operatorUserId;
        OperatorName = operatorName;
        RequestPath = requestPath;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public static SystemOperationLog Create(
        string entityType,
        string entityId,
        string actionType,
        string changeJson,
        string operatorUserId,
        string operatorName,
        string requestPath,
        string ipAddress,
        string userAgent)
    {
        return new SystemOperationLog(entityType, entityId, actionType, changeJson, operatorUserId, operatorName, requestPath, ipAddress, userAgent);
    }
}
```

- [ ] **Step 2: Expose the DbSet through the application interface**

In `src/2.QPS.Application/Interfaces/IDbContext.cs`, add this property near the other system DbSets:

```csharp
DbSet<SystemOperationLog> SystemOperationLogs { get; }
```

The existing `using QPS.Domain.Entities.System;` already covers the type.

- [ ] **Step 3: Add DbSet and model configuration**

In `src/3.QPS.Infrastructure/Database/AppDbContext.cs`, add:

```csharp
public DbSet<SystemOperationLog> SystemOperationLogs { get; set; }
```

Inside `OnModelCreating`, add:

```csharp
modelBuilder.Entity<SystemOperationLog>(entity =>
{
    entity.HasIndex(log => new { log.EntityType, log.EntityId, log.CreatedAt });
    entity.HasIndex(log => new { log.OperatorUserId, log.CreatedAt });
    entity.HasIndex(log => new { log.ActionType, log.CreatedAt });

    entity.Property(log => log.EntityType).HasMaxLength(100);
    entity.Property(log => log.EntityId).HasMaxLength(64);
    entity.Property(log => log.ActionType).HasMaxLength(50);
    entity.Property(log => log.OperatorUserId).HasMaxLength(64);
    entity.Property(log => log.OperatorName).HasMaxLength(100);
    entity.Property(log => log.RequestPath).HasMaxLength(300);
    entity.Property(log => log.IpAddress).HasMaxLength(64);
    entity.Property(log => log.UserAgent).HasMaxLength(500);
});
```

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln"
```

Expected: build passes.

- [ ] **Step 5: Commit**

```powershell
git -C "E:\Code\QPS\QPS-HT" add "src/1.QPS.Domain/Entities/System/SystemOperationLog.cs" "src/2.QPS.Application/Interfaces/IDbContext.cs" "src/3.QPS.Infrastructure/Database/AppDbContext.cs"
git -C "E:\Code\QPS\QPS-HT" commit -m "feat: add system operation log entity"
```

---

### Task 2: Request Context Metadata

**Files:**
- Modify: `src/2.QPS.Application/Interfaces/ICurrentUserService.cs`
- Modify: `src/3.QPS.Infrastructure/Identity/CurrentUserService.cs`

**Interfaces:**
- Consumes: existing `IHttpContextAccessor`
- Produces: `ICurrentUserService.RequestPath`
- Produces: `ICurrentUserService.IpAddress`
- Produces: `ICurrentUserService.UserAgent`

- [ ] **Step 1: Extend the interface**

In `ICurrentUserService`, add:

```csharp
string? RequestPath { get; }
string? IpAddress { get; }
string? UserAgent { get; }
```

- [ ] **Step 2: Implement the properties**

In `CurrentUserService`, add:

```csharp
public string? RequestPath => _httpContextAccessor.HttpContext?.Request?.Path.Value;

public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

public string? UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString();
```

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln"
```

Expected: build passes.

- [ ] **Step 4: Commit**

```powershell
git -C "E:\Code\QPS\QPS-HT" add "src/2.QPS.Application/Interfaces/ICurrentUserService.cs" "src/3.QPS.Infrastructure/Identity/CurrentUserService.cs"
git -C "E:\Code\QPS\QPS-HT" commit -m "feat: expose request context for operation logs"
```

---

### Task 3: Automatic Log Collection In AppDbContext

**Files:**
- Modify: `src/3.QPS.Infrastructure/Database/AppDbContext.cs`

**Interfaces:**
- Consumes: `SystemOperationLog.Create(...)`
- Consumes: `ICurrentUserService.UserId`, `Username`, `RequestPath`, `IpAddress`, `UserAgent`
- Produces: automatic logs during `SaveChanges()` and `SaveChangesAsync(...)`

- [ ] **Step 1: Add usings**

At the top of `AppDbContext.cs`, add:

```csharp
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
```

- [ ] **Step 2: Change save pipeline**

Replace current save methods with:

```csharp
public override int SaveChanges()
{
    var operationLogs = CollectOperationLogs();
    SetAuditFields();
    SystemOperationLogs.AddRange(operationLogs);
    return base.SaveChanges();
}

public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var operationLogs = CollectOperationLogs();
    SetAuditFields();
    await SystemOperationLogs.AddRangeAsync(operationLogs, cancellationToken);
    return await base.SaveChangesAsync(cancellationToken);
}
```

- [ ] **Step 3: Add collection helpers**

Add these private members to `AppDbContext`:

```csharp
private static readonly HashSet<string> IgnoredOperationLogFields = new(StringComparer.OrdinalIgnoreCase)
{
    nameof(BaseEntity.Id),
    nameof(BaseEntity.CreatedAt),
    nameof(BaseEntity.CreatedBy),
    nameof(BaseEntity.UpdatedAt),
    nameof(BaseEntity.UpdatedBy),
    nameof(BaseEntity.IsDeleted),
    "Password",
    "PasswordHash",
    "Token",
    "RefreshToken"
};

private static readonly HashSet<string> StatusFields = new(StringComparer.OrdinalIgnoreCase)
{
    "Status",
    "State",
    "Enabled",
    "IsEnabled",
    "IsActive"
};

private static readonly HashSet<string> OwnerFields = new(StringComparer.OrdinalIgnoreCase)
{
    "OwnerUserId",
    "AssigneeId",
    "ResponsibleUserId",
    "ManagerUserId"
};

private List<SystemOperationLog> CollectOperationLogs()
{
    ChangeTracker.DetectChanges();

    return ChangeTracker.Entries()
        .Where(entry => entry.Entity is BaseEntity)
        .Where(entry => entry.Entity is not SystemOperationLog)
        .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
        .Select(CreateOperationLog)
        .Where(log => log is not null)
        .Cast<SystemOperationLog>()
        .ToList();
}

private SystemOperationLog? CreateOperationLog(EntityEntry entry)
{
    var changeMap = BuildChangeMap(entry);
    if (changeMap.Count == 0)
    {
        return null;
    }

    var entity = (BaseEntity)entry.Entity;
    var actionType = ResolveActionType(entry.State, changeMap.Keys);
    var changeJson = JsonSerializer.Serialize(changeMap);

    return SystemOperationLog.Create(
        entry.Metadata.ClrType.Name,
        entity.Id.ToString(),
        actionType,
        changeJson,
        _currentUserService.UserId ?? string.Empty,
        _currentUserService.Username ?? "System",
        _currentUserService.RequestPath ?? string.Empty,
        _currentUserService.IpAddress ?? string.Empty,
        _currentUserService.UserAgent ?? string.Empty);
}

private static Dictionary<string, OperationLogChange> BuildChangeMap(EntityEntry entry)
{
    var changes = new Dictionary<string, OperationLogChange>();

    foreach (var property in entry.Properties)
    {
        var propertyName = property.Metadata.Name;
        if (IgnoredOperationLogFields.Contains(propertyName))
        {
            continue;
        }

        if (entry.State == EntityState.Modified && !property.IsModified)
        {
            continue;
        }

        var oldValue = entry.State == EntityState.Added ? null : property.OriginalValue;
        var newValue = entry.State == EntityState.Deleted ? null : property.CurrentValue;

        if (entry.State == EntityState.Modified && Equals(oldValue, newValue))
        {
            continue;
        }

        changes[propertyName] = new OperationLogChange(oldValue, newValue);
    }

    return changes;
}

private static string ResolveActionType(EntityState state, IEnumerable<string> changedFields)
{
    if (state == EntityState.Added)
    {
        return "Create";
    }

    if (state == EntityState.Deleted)
    {
        return "Delete";
    }

    var fields = changedFields.ToList();
    if (fields.Count > 0 && fields.All(StatusFields.Contains))
    {
        return "StatusChange";
    }

    if (fields.Count > 0 && fields.All(OwnerFields.Contains))
    {
        return "AssignOwner";
    }

    return "Update";
}

private sealed record OperationLogChange(object? Old, object? New);
```

- [ ] **Step 4: Verify JSON property casing**

If product wants lowercase `old/new`, change the record to:

```csharp
private sealed record OperationLogChange(
    [property: System.Text.Json.Serialization.JsonPropertyName("old")] object? Old,
    [property: System.Text.Json.Serialization.JsonPropertyName("new")] object? New);
```

Use lowercase because the design spec examples use `"old"` and `"new"`.

- [ ] **Step 5: Build**

Run:

```powershell
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln"
```

Expected: build passes.

- [ ] **Step 6: Commit**

```powershell
git -C "E:\Code\QPS\QPS-HT" add "src/3.QPS.Infrastructure/Database/AppDbContext.cs"
git -C "E:\Code\QPS\QPS-HT" commit -m "feat: collect operation logs automatically"
```

---

### Task 4: Query API

**Files:**
- Create: `src/2.QPS.Application/Contracts/System/OperationLogs/OperationLogDto.cs`
- Create: `src/2.QPS.Application/Contracts/System/OperationLogs/OperationLogQueryRequest.cs`
- Create: `src/2.QPS.Application/Features/System/OperationLogs/GetOperationLogsQuery.cs`
- Create: `src/4.QPS.WebAPI/Controllers/Admin/System/OperationLogController.cs`

**Interfaces:**
- Consumes: `IDbContext.SystemOperationLogs`
- Produces: `GET /api/admin/operation-logs`

- [ ] **Step 1: Add DTO**

Create `OperationLogDto.cs`:

```csharp
namespace QPS.Application.Contracts.System.OperationLogs;

public class OperationLogDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ChangeJson { get; set; } = string.Empty;
    public string OperatorUserId { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 2: Add query request**

Create `OperationLogQueryRequest.cs`:

```csharp
using QPS.Application.Extensions;

namespace QPS.Application.Contracts.System.OperationLogs;

public class OperationLogQueryRequest : PaginationRequest
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public string? OperatorUserId { get; set; }
    public string? OperatorName { get; set; }
    public string? RequestPath { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}
```

- [ ] **Step 3: Add MediatR query**

Create `GetOperationLogsQuery.cs`:

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.OperationLogs;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.System.OperationLogs;

public class GetOperationLogsQuery : IRequest<PaginationResponse<OperationLogDto>>
{
    public OperationLogQueryRequest Request { get; set; } = new();
}

public class GetOperationLogsHandler : IRequestHandler<GetOperationLogsQuery, PaginationResponse<OperationLogDto>>
{
    private readonly IDbContext _dbContext;

    public GetOperationLogsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<OperationLogDto>> Handle(GetOperationLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.SystemOperationLogs.AsNoTracking();
        var filter = request.Request;

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(log => log.EntityType == filter.EntityType);
        if (!string.IsNullOrWhiteSpace(filter.EntityId))
            query = query.Where(log => log.EntityId == filter.EntityId);
        if (!string.IsNullOrWhiteSpace(filter.ActionType))
            query = query.Where(log => log.ActionType == filter.ActionType);
        if (!string.IsNullOrWhiteSpace(filter.OperatorUserId))
            query = query.Where(log => log.OperatorUserId == filter.OperatorUserId);
        if (!string.IsNullOrWhiteSpace(filter.OperatorName))
            query = query.Where(log => log.OperatorName.Contains(filter.OperatorName));
        if (!string.IsNullOrWhiteSpace(filter.RequestPath))
            query = query.Where(log => log.RequestPath.Contains(filter.RequestPath));
        if (filter.StartAt.HasValue)
            query = query.Where(log => log.CreatedAt >= filter.StartAt.Value);
        if (filter.EndAt.HasValue)
            query = query.Where(log => log.CreatedAt <= filter.EndAt.Value);

        return await query
            .OrderByDescending(log => log.CreatedAt)
            .Select(log => new OperationLogDto
            {
                Id = log.Id,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                ActionType = log.ActionType,
                ChangeJson = log.ChangeJson,
                OperatorUserId = log.OperatorUserId,
                OperatorName = log.OperatorName,
                RequestPath = log.RequestPath,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            })
            .ToPaginationResponseAsync(filter.PageNum, filter.PageSize, cancellationToken);
    }
}
```

- [ ] **Step 4: Add controller**

Create `OperationLogController.cs`:

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.System.OperationLogs;
using QPS.Application.Extensions;
using QPS.Application.Features.System.OperationLogs;

namespace QPS.WebAPI.Controllers.Admin.System;

[ApiController]
[Route("api/admin/operation-logs")]
[Authorize]
public class OperationLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationLogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<OperationLogDto>>> GetOperationLogs([FromQuery] OperationLogQueryRequest request)
    {
        var query = new GetOperationLogsQuery { Request = request };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

- [ ] **Step 5: Build**

Run:

```powershell
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln"
```

Expected: build passes.

- [ ] **Step 6: Commit**

```powershell
git -C "E:\Code\QPS\QPS-HT" add "src/2.QPS.Application/Contracts/System/OperationLogs" "src/2.QPS.Application/Features/System/OperationLogs" "src/4.QPS.WebAPI/Controllers/Admin/System/OperationLogController.cs"
git -C "E:\Code\QPS\QPS-HT" commit -m "feat: add operation log query api"
```

---

### Task 5: Verification In CodexTemp

**Files:**
- Create: `..\CodexTemp\SystemOperationLogChecks\SystemOperationLogChecks.csproj`
- Create: `..\CodexTemp\SystemOperationLogChecks\Program.cs`

**Interfaces:**
- Consumes: `AppDbContext`
- Consumes: `SystemOperationLog`
- Produces: repeatable smoke checks outside the repository

- [ ] **Step 1: Create verification project outside repo source**

Run:

```powershell
New-Item -ItemType Directory -Force -Path "E:\Code\QPS\CodexTemp\SystemOperationLogChecks" | Out-Null
dotnet new console -n SystemOperationLogChecks -o "E:\Code\QPS\CodexTemp\SystemOperationLogChecks" --framework net8.0
dotnet add "E:\Code\QPS\CodexTemp\SystemOperationLogChecks\SystemOperationLogChecks.csproj" reference "E:\Code\QPS\QPS-HT\src\1.QPS.Domain\QPS.Domain.csproj"
dotnet add "E:\Code\QPS\CodexTemp\SystemOperationLogChecks\SystemOperationLogChecks.csproj" reference "E:\Code\QPS\QPS-HT\src\2.QPS.Application\QPS.Application.csproj"
dotnet add "E:\Code\QPS\CodexTemp\SystemOperationLogChecks\SystemOperationLogChecks.csproj" reference "E:\Code\QPS\QPS-HT\src\3.QPS.Infrastructure\QPS.Infrastructure.csproj"
dotnet add "E:\Code\QPS\CodexTemp\SystemOperationLogChecks\SystemOperationLogChecks.csproj" package Microsoft.EntityFrameworkCore.InMemory --version 8.0.4
```

- [ ] **Step 2: Add smoke check code**

Write `Program.cs` with checks that:

```csharp
// Arrange an AppDbContext with InMemory provider and fake ICurrentUserService.
// Add a CrmCustomer, save, assert one Create log exists.
// Change Status only, save, assert StatusChange.
// Change OwnerUserId only, save, assert AssignOwner.
// Change CustomerName, save, assert Update and ChangeJson contains CustomerName old/new.
// Add and save a SystemOperationLog directly, assert no recursive extra log is created.
```

Use a small fake service:

```csharp
private sealed class FakeCurrentUserService : ICurrentUserService
{
    public string? UserId => "user-1";
    public string? Username => "tester";
    public string? RequestPath => "/api/test";
    public string? IpAddress => "127.0.0.1";
    public string? UserAgent => "CodexTemp";
}
```

- [ ] **Step 3: Run verification**

Run:

```powershell
dotnet run --project "E:\Code\QPS\CodexTemp\SystemOperationLogChecks\SystemOperationLogChecks.csproj"
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln"
```

Expected: smoke checks print `PASS`; solution build passes.

- [ ] **Step 4: Commit only repo source changes if verification forced fixes**

If Task 5 required source fixes:

```powershell
git -C "E:\Code\QPS\QPS-HT" status --short
git -C "E:\Code\QPS\QPS-HT" add <only-fixed-source-files>
git -C "E:\Code\QPS\QPS-HT" commit -m "fix: verify operation log behavior"
```

Do not add `E:\Code\QPS\CodexTemp\SystemOperationLogChecks`.

---

## Self-Review

- Spec coverage: entity fields, removed fields, automatic `ActionType`, automatic `ChangeJson`, request metadata, recursion prevention, and success-only save behavior are covered.
- Placeholder scan: no unresolved markers or placeholder steps.
- Type consistency: plan consistently uses `SystemOperationLog`, `SystemOperationLogs`, `OperationLogDto`, `OperationLogQueryRequest`, and `GetOperationLogsQuery`.
