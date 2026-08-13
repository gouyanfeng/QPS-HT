# 基地主体评分领域事件实施计划

> **给执行代理的要求：** 实施本计划时必须使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans`。每一步用复选框跟踪。

**目标：** 用领域事件替换各个 Command 里重复的“保存业务数据、组装评分输入、重算评分、再次保存”代码。

**架构：** 领域实体在业务方法内部触发“基地主体评分受影响”事件；`AppDbContext.SaveChangesAsync` 先保存原业务数据，再发布领域事件；Application 层事件处理器统一查询评分输入，调用 `subject.RecalculateScoreGrade(input)` 并保存 `Score/Grade`。`RecalculateScoreGrade` 只覆盖结果，不触发事件，避免递归。

**技术栈：** .NET 8、EF Core 8、MediatR 14、xUnit、现有 QPS 分层架构。

## 全局约束

- 只处理服务端 `QPS-HT`。
- 不恢复 `CrmHerbBaseSubjectScoreService`。
- Command 不再直接调用 `CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(...)`。
- Command 不再直接调用 `subject.RecalculateScoreGrade(...)`。
- Domain 不查数据库，不依赖 Application。
- 事件由业务方法触发，不在 Command 里写“请求重算评分”这种技术语义。
- 通用属性表 `CrmBusinessEntityAttribute` 不直接承载评分规则；主营品类变化由 Application 找到主体后调用主体业务方法触发事件。
- `RecalculateScoreGrade(...)` 不触发任何领域事件。
- 不新增仓库内测试文件；如需临时实验，放 `CodexTemp`。

---

## 文件结构

- 新增：`src/1.QPS.Domain/Events/Crm/CrmHerbBaseSubjectScoreAffectedEvent.cs`
  - 定义“基地主体评分受影响”领域事件。
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmHerbBaseSubject.cs`
  - 主体自身业务变化触发评分事件。
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmHerbBase.cs`
  - 基地变化触发其所属主体评分事件。
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmContact.cs`
  - 主体联系人变化触发评分事件。
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmFollowRecord.cs`
  - 新增跟进记录触发主体评分事件。
- 新增：`src/2.QPS.Application/Features/Crm/CrmHerbBaseSubjects/RecalculateCrmHerbBaseSubjectScoreEventHandler.cs`
  - 统一处理评分事件。
- 修改：`src/3.QPS.Infrastructure/Database/AppDbContext.cs`
  - 保存后发布领域事件。
- 修改：`src/2.QPS.Application/Features/Crm/**`
  - 删除 Command 中直接重算评分的重复代码。
- 修改：`tests/QPS.UnitTests/Common/TestDbContextFactory.cs`
  - 测试环境支持事件发布。
- 修改：现有 CRM 单测文件
  - 验证事件驱动后评分仍会更新。

---

## 任务 1：新增评分受影响领域事件

**文件：**
- 新增：`src/1.QPS.Domain/Events/Crm/CrmHerbBaseSubjectScoreAffectedEvent.cs`
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmHerbBaseSubject.cs`

**产出接口：**
- `CrmHerbBaseSubjectScoreAffectedEvent(Guid subjectId)`
- `CrmHerbBaseSubject.NotifyScoreAffected()`

- [ ] **步骤 1：新增事件类**

创建 `src/1.QPS.Domain/Events/Crm/CrmHerbBaseSubjectScoreAffectedEvent.cs`：

```csharp
using QPS.Domain.Common;

namespace QPS.Domain.Events.Crm;

public sealed class CrmHerbBaseSubjectScoreAffectedEvent : DomainEvent
{
    public CrmHerbBaseSubjectScoreAffectedEvent(Guid subjectId)
    {
        SubjectId = subjectId;
    }

    public Guid SubjectId { get; }
}
```

- [ ] **步骤 2：主体实体增加事件触发方法**

在 `CrmHerbBaseSubject.cs` 增加引用：

```csharp
using QPS.Domain.Events.Crm;
```

在 `CrmHerbBaseSubject` 内增加：

```csharp
public void NotifyScoreAffected()
{
    AddDomainEvent(new CrmHerbBaseSubjectScoreAffectedEvent(Id));
}
```

- [ ] **步骤 3：主体业务方法内部触发事件**

在这些方法末尾调用：

```csharp
NotifyScoreAffected();
```

需要处理的方法：

- `UpdateBasicInfo(...)`
- `UpdateScale(...)`
- `UpdatePrimaryContact(...)`
- `ClearPrimaryContact()`
- `UpdateFollowSummary(...)`

禁止在 `RecalculateScoreGrade(...)` 中调用 `NotifyScoreAffected()`。

- [ ] **步骤 4：构建 Domain 项目**

运行：

```powershell
dotnet build src\1.QPS.Domain\QPS.Domain.csproj --no-restore
```

期望：构建成功。

---

## 任务 2：在 AppDbContext 保存后发布领域事件

**文件：**
- 修改：`src/3.QPS.Infrastructure/Database/AppDbContext.cs`
- 修改：`tests/QPS.UnitTests/Common/TestDbContextFactory.cs`

**产出接口：**
- `AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService, IPublisher? publisher = null)`
- `CollectDomainEvents()`

- [ ] **步骤 1：给 AppDbContext 注入发布器**

在 `AppDbContext.cs` 增加引用：

```csharp
using MediatR;
```

增加字段：

```csharp
private readonly IPublisher? _publisher;
private bool _isPublishingDomainEvents;
```

替换构造函数：

```csharp
public AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUserService,
    IPublisher? publisher = null) : base(options)
{
    _currentUserService = currentUserService;
    _publisher = publisher;
}
```

- [ ] **步骤 2：收集并清空领域事件**

在 `AppDbContext` 内新增：

```csharp
private List<DomainEvent> CollectDomainEvents()
{
    var entities = ChangeTracker.Entries<BaseEntity>()
        .Select(entry => entry.Entity)
        .Where(entity => entity.GetDomainEvents().Count > 0)
        .ToList();

    var domainEvents = entities
        .SelectMany(entity => entity.GetDomainEvents())
        .ToList();

    foreach (var entity in entities)
    {
        entity.ClearDomainEvents();
    }

    return domainEvents;
}
```

- [ ] **步骤 3：改造异步保存流程**

替换 `SaveChangesAsync`：

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    SetAuditFields();
    AddOperationLogs();
    SetAuditFields();

    if (_isPublishingDomainEvents)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    var domainEvents = CollectDomainEvents();
    var result = await base.SaveChangesAsync(cancellationToken);

    if (_publisher == null || domainEvents.Count == 0)
    {
        return result;
    }

    _isPublishingDomainEvents = true;
    try
    {
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
    finally
    {
        _isPublishingDomainEvents = false;
    }

    return result;
}
```

同步 `SaveChanges()` 暂不发布事件；现有 Command 使用异步保存。

- [ ] **步骤 4：测试工厂支持发布器**

在 `TestDbContextFactory.cs` 增加引用：

```csharp
using MediatR;
```

修改 `Create` 方法签名：

```csharp
public static AppDbContext Create(
    ICurrentUserService? currentUserService = null,
    IPublisher? publisher = null)
```

返回：

```csharp
return new AppDbContext(options, currentUserService ?? new TestCurrentUserService(), publisher);
```

- [ ] **步骤 5：构建后端**

运行：

```powershell
dotnet build QPS.sln --no-restore
```

期望：构建成功。

---

## 任务 3：新增评分事件处理器

**文件：**
- 新增：`src/2.QPS.Application/Features/Crm/CrmHerbBaseSubjects/RecalculateCrmHerbBaseSubjectScoreEventHandler.cs`

**依赖接口：**
- `CrmHerbBaseSubjectScoreAffectedEvent.SubjectId`
- `CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(...)`
- `CrmHerbBaseSubject.RecalculateScoreGrade(...)`

- [ ] **步骤 1：新增处理器**

创建 `RecalculateCrmHerbBaseSubjectScoreEventHandler.cs`：

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Events.Crm;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public sealed class RecalculateCrmHerbBaseSubjectScoreEventHandler
    : INotificationHandler<CrmHerbBaseSubjectScoreAffectedEvent>
{
    private readonly IDbContext _dbContext;

    public RecalculateCrmHerbBaseSubjectScoreEventHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        CrmHerbBaseSubjectScoreAffectedEvent notification,
        CancellationToken cancellationToken)
    {
        var scoreInput = await CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(
            _dbContext,
            notification.SubjectId,
            cancellationToken);

        if (scoreInput == null)
        {
            return;
        }

        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(item => item.Id == notification.SubjectId, cancellationToken);

        if (subject == null)
        {
            return;
        }

        subject.RecalculateScoreGrade(scoreInput);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **步骤 2：确认 MediatR 扫描范围**

检查 `src/4.QPS.WebAPI/Program.cs` 中 `AddMediatR` 是否扫描 Application 程序集。期望包含 `RecalculateCrmHerbBaseSubjectScoreEventHandler` 所在程序集。

- [ ] **步骤 3：构建 Application 项目**

运行：

```powershell
dotnet build src\2.QPS.Application\QPS.Application.csproj --no-restore
```

期望：构建成功。

---

## 任务 4：基地、联系人、跟进记录触发领域事件

**文件：**
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmHerbBase.cs`
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmContact.cs`
- 修改：`src/1.QPS.Domain/Entities/Crm/CrmFollowRecord.cs`

**依赖接口：**
- `CrmHerbBaseSubjectScoreAffectedEvent(Guid subjectId)`

- [ ] **步骤 1：先列出领域方法**

运行：

```powershell
rg -n "Create\\(|Update|Delete|IsDeleted|MarkPrimary|UnmarkPrimary|UpdateStatus|CrmFollowRecord" src\1.QPS.Domain\Entities\Crm
```

期望：确认当前 Command 实际调用的领域方法。

- [ ] **步骤 2：基地实体触发事件**

在 `CrmHerbBase.cs` 增加引用：

```csharp
using QPS.Domain.Events.Crm;
```

增加私有方法：

```csharp
private void NotifySubjectScoreAffected()
{
    if (HerbBaseSubjectId.HasValue)
    {
        AddDomainEvent(new CrmHerbBaseSubjectScoreAffectedEvent(HerbBaseSubjectId.Value));
    }
}
```

在会影响评分输入的方法末尾调用：

```csharp
NotifySubjectScoreAffected();
```

需要覆盖：基础信息、来源、主联系人、状态、关联主体、软删除。

- [ ] **步骤 3：联系人实体触发事件**

在 `CrmContact.cs` 增加引用：

```csharp
using QPS.Domain.Events.Crm;
```

增加私有方法：

```csharp
private void NotifySubjectScoreAffected()
{
    if (EntityType == "CRM_HERB_BASE_SUBJECT")
    {
        AddDomainEvent(new CrmHerbBaseSubjectScoreAffectedEvent(EntityId));
    }
}
```

在会影响评分输入的方法末尾调用：

```csharp
NotifySubjectScoreAffected();
```

需要覆盖：联系人姓名、电话、主联系人标记、状态。

- [ ] **步骤 4：跟进记录创建时触发事件**

在 `CrmFollowRecord.cs` 增加引用：

```csharp
using QPS.Domain.Events.Crm;
```

在设置 `HerbBaseSubjectId` 后触发：

```csharp
AddDomainEvent(new CrmHerbBaseSubjectScoreAffectedEvent(HerbBaseSubjectId));
```

- [ ] **步骤 5：构建 Domain 项目**

运行：

```powershell
dotnet build src\1.QPS.Domain\QPS.Domain.csproj --no-restore
```

期望：构建成功。

---

## 任务 5：删除 Command 中直接重算评分的代码

**文件：**
- 修改：`src/2.QPS.Application/Features/Crm/CrmHerbBaseSubjects/UpdateCrmHerbBaseSubjectCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmHerbBases/CreateCrmHerbBaseCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmHerbBases/UpdateCrmHerbBaseCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmHerbBases/DeleteCrmHerbBaseCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmFollowRecords/CreateCrmFollowRecordCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmContacts/CreateCrmContactCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmContacts/UpdateCrmContactCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmContacts/UpdateCrmContactStatusCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmContacts/SetPrimaryCrmContactCommand.cs`

**删除内容：**
- `CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(...)`
- `subject.RecalculateScoreGrade(...)`
- 围绕评分重算产生的第二次 `SaveChangesAsync(...)`

- [ ] **步骤 1：列出所有直接重算点**

运行：

```powershell
rg -n "CrmHerbBaseSubjectScoreInputBuilder\\.BuildAsync|RecalculateScoreGrade\\(" src\2.QPS.Application\Features\Crm
```

期望：看到当前重复代码所在 Command。

- [ ] **步骤 2：逐个 Command 删除重复块**

把类似下面的代码块删除：

```csharp
await _dbContext.SaveChangesAsync(cancellationToken);
var scoreInput = await CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(_dbContext, subject.Id, cancellationToken);
if (scoreInput != null)
{
    subject.RecalculateScoreGrade(scoreInput);
}
await _dbContext.SaveChangesAsync(cancellationToken);
```

保留业务保存：

```csharp
await _dbContext.SaveChangesAsync(cancellationToken);
```

- [ ] **步骤 3：删除不再使用的 using**

运行：

```powershell
rg -n "using QPS.Application.Features.Crm;" src\2.QPS.Application\Features\Crm
```

对只为评分 Builder 引入的文件，删除该 using。

- [ ] **步骤 4：确认只剩事件处理器直接重算**

运行：

```powershell
rg -n "CrmHerbBaseSubjectScoreInputBuilder\\.BuildAsync|RecalculateScoreGrade\\(" src\2.QPS.Application\Features\Crm
```

期望：只有 `RecalculateCrmHerbBaseSubjectScoreEventHandler.cs` 里还出现这些调用。

- [ ] **步骤 5：构建 Application 项目**

运行：

```powershell
dotnet build src\2.QPS.Application\QPS.Application.csproj --no-restore
```

期望：构建成功。

---

## 任务 6：主营品类属性变化触发主体事件

**文件：**
- 修改：`src/2.QPS.Application/Features/Crm/CrmBusinessEntityAttributes/SaveCrmBusinessEntityAttributesCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmHerbBases/CreateCrmHerbBaseCommand.cs`
- 修改：`src/2.QPS.Application/Features/Crm/CrmHerbBases/UpdateCrmHerbBaseCommand.cs`

**依赖接口：**
- `CrmHerbBaseSubject.NotifyScoreAffected()`

- [ ] **步骤 1：保持属性实体通用**

不要在 `CrmBusinessEntityAttribute` 实体中加入评分事件。它是通用属性表，不知道“主营品类影响基地主体评分”。

- [ ] **步骤 2：基地创建和更新同步主营品类后触发主体事件**

如果当前基地有关联主体：

```csharp
if (herbBase.HerbBaseSubjectId.HasValue)
{
    var subject = await _dbContext.CrmHerbBaseSubjects
        .FirstOrDefaultAsync(item => item.Id == herbBase.HerbBaseSubjectId.Value, cancellationToken);

    subject?.NotifyScoreAffected();
}
```

- [ ] **步骤 3：通用属性保存命令识别主营品类**

在 `SaveCrmBusinessEntityAttributesCommand` 中，当满足：

```csharp
request.Request.EntityType == CrmCodes.HerbBaseEntityType
&& request.Request.AttributeCode == CrmCodes.MainProductAttributeCode
```

加载基地和主体：

```csharp
var herbBase = await _dbContext.CrmHerbBases
    .FirstOrDefaultAsync(item => item.Id == request.Request.EntityId, cancellationToken);

if (herbBase?.HerbBaseSubjectId.HasValue == true)
{
    var subject = await _dbContext.CrmHerbBaseSubjects
        .FirstOrDefaultAsync(item => item.Id == herbBase.HerbBaseSubjectId.Value, cancellationToken);

    subject?.NotifyScoreAffected();
}
```

- [ ] **步骤 4：构建 Application 项目**

运行：

```powershell
dotnet build src\2.QPS.Application\QPS.Application.csproj --no-restore
```

期望：构建成功。

---

## 任务 7：补充事件驱动评分测试

**文件：**
- 修改：`tests/QPS.UnitTests/Common/TestDbContextFactory.cs`
- 修改：`tests/QPS.UnitTests/Features/Crm/CrmHerbBases/CrmHerbBaseCommandTests.cs`
- 修改：`tests/QPS.UnitTests/Features/Crm/CrmContacts/CrmContactCommandTests.cs`

**测试目标：**
- Command 不直接重算评分。
- 业务保存后，领域事件处理器仍会更新主体 `Score/Grade`。

- [ ] **步骤 1：测试工厂增加真实 MediatR 发布器**

在 `TestDbContextFactory.cs` 增加引用：

```csharp
using Microsoft.Extensions.DependencyInjection;
using QPS.Application.Features.Crm.CrmHerbBaseSubjects;
```

新增方法：

```csharp
public static AppDbContext CreateWithPublisher(ICurrentUserService? currentUserService = null)
{
    var services = new ServiceCollection();

    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(RecalculateCrmHerbBaseSubjectScoreEventHandler).Assembly));

    services.AddScoped<ICurrentUserService>(_ => currentUserService ?? new TestCurrentUserService());

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    services.AddScoped(_ => options);
    services.AddScoped<AppDbContext>();
    services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());

    var provider = services.BuildServiceProvider();
    return provider.GetRequiredService<AppDbContext>();
}
```

- [ ] **步骤 2：基地创建后通过事件更新评分**

在 `CrmHerbBaseCommandTests.cs` 增加测试：

```csharp
[Fact]
public async Task Create_ShouldRecalculateSubjectScoreThroughDomainEvent()
{
    await using var dbContext = TestDbContextFactory.CreateWithPublisher();
    var handler = new CreateCrmHerbBaseHandler(dbContext);

    await handler.Handle(new CreateCrmHerbBaseCommand
    {
        Request = new CrmHerbBaseCreateRequest
        {
            HerbBaseName = "事件评分基地",
            MainProducts = new List<string> { "黄芪" },
            Province = "甘肃",
            City = "定西",
            Area = "陇西",
            Address = "测试地址",
            Scale = 120,
            SourcePlatform = "MANUAL",
            PrimaryContactName = "张三",
            PrimaryContactPhone = "13900000000",
            Remark = "事件评分测试"
        }
    }, CancellationToken.None);

    var subject = await dbContext.CrmHerbBaseSubjects.SingleAsync(item => item.SubjectName == "事件评分基地");

    Assert.True(subject.Score > 0);
    Assert.Contains(subject.Grade, new[] { "高", "中", "低", "无效" });
}
```

- [ ] **步骤 3：联系人更新后通过事件更新评分**

在 `CrmContactCommandTests.cs` 增加测试：创建主体和联系人，记录原始 `Score`，通过联系人更新 Command 修改有效电话或主联系人，再断言评分变化。

断言结构：

```csharp
var originalScore = subject.Score;

await handler.Handle(command, CancellationToken.None);

Assert.NotEqual(originalScore, subject.Score);
```

- [ ] **步骤 4：运行 CRM 单测**

运行：

```powershell
dotnet test tests\QPS.UnitTests\QPS.UnitTests.csproj --no-restore --filter "FullyQualifiedName~QPS.UnitTests.Features.Crm"
```

期望：测试通过。

---

## 任务 8：最终验证

**文件：**
- 无固定修改文件；只在验证失败时做最小修复。

- [ ] **步骤 1：确认 Command 没有直接重算评分**

运行：

```powershell
rg -n "CrmHerbBaseSubjectScoreInputBuilder\\.BuildAsync|RecalculateScoreGrade\\(" src tests -g "!**/bin/**" -g "!**/obj/**"
```

期望结果只包含：

```text
src\1.QPS.Domain\Entities\Crm\CrmHerbBaseSubject.cs:<行号>: public void RecalculateScoreGrade(...)
src\2.QPS.Application\Features\Crm\CrmHerbBaseSubjects\RecalculateCrmHerbBaseSubjectScoreEventHandler.cs:<行号>: CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(...)
src\2.QPS.Application\Features\Crm\CrmHerbBaseSubjects\RecalculateCrmHerbBaseSubjectScoreEventHandler.cs:<行号>: subject.RecalculateScoreGrade(scoreInput);
```

- [ ] **步骤 2：构建后端**

运行：

```powershell
dotnet build QPS.sln --no-restore
```

期望：构建成功。

- [ ] **步骤 3：运行 CRM 单测**

运行：

```powershell
dotnet test tests\QPS.UnitTests\QPS.UnitTests.csproj --no-restore --filter "FullyQualifiedName~QPS.UnitTests.Features.Crm"
```

期望：测试通过。

- [ ] **步骤 4：检查操作日志影响**

运行一个带事件发布的新增或修改 Command 测试，查看 `SystemOperationLogs`。预期可能出现两类日志：

- 原始业务数据变化日志。
- 评分字段变化日志。

如果评分字段日志太吵，另起任务决定是否在操作日志里过滤 `Score/Grade` 的纯派生字段更新；本计划不处理这个策略。

---

## 自查结果

- 覆盖范围：事件定义、发布机制、处理器、Command 去重、主营品类特殊处理、测试和验证都已覆盖。
- 占位符检查：没有未完成占位符。
- 命名一致性：统一使用 `CrmHerbBaseSubjectScoreAffectedEvent` 和 `NotifyScoreAffected()`。
- 范围控制：只处理服务端 CRM 基地主体评分，不处理前端，不处理 ponytail audit 的其他项。
