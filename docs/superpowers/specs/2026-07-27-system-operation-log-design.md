# 通用操作日志设计

## 背景

系统当前已有错误日志 `SystemErrorLog`，用于记录异常；也已有 `BaseEntity` 审计字段和 `AppDbContext.SetAuditFields()`，用于维护创建、修改信息。新的操作日志不替代错误日志，也不承担实体审计字段职责，而是记录业务数据变更事实，供后台审计列表和业务详情时间线查询使用。

## 目标

- 提供通用操作日志能力，不绑定 CRM 或某个具体模块。
- 业务代码尽量无感知，不要求在每个 Command 或 Service 中显式指定操作类型。
- 自动根据 EF `ChangeTracker` 计算 `ActionType` 和 `ChangeJson`。
- 字段结构保持精简，只保存结构化事实，不保存展示摘要或冗余快照。

## 非目标

- 不记录失败操作；失败仍由现有 `SystemErrorLog` 处理。
- 不保存 `ModuleName`、`ActionName`、`Summary`、`ChangedFields`、`BeforeJson`、`AfterJson`。
- 不保存 `RequestMethod`。
- 第一版不做复杂业务动作识别，例如导入、导出、移动、排序、设为默认。

## 数据模型

新增系统实体 `SystemOperationLog`，建议放在 `QPS.Domain.Entities.System`。

字段：

| 字段 | 类型建议 | 说明 |
| --- | --- | --- |
| Id | Guid | 主键 |
| EntityType | string | 实体类型，如 `CrmCustomer`、`SystemUser` |
| EntityId | string | 实体 ID，统一字符串化，兼容 Guid、int、long |
| ActionType | string | 自动推断的操作类型 |
| ChangeJson | string | 字段级变更明细 JSON |
| OperatorUserId | string | 当前操作人 ID |
| OperatorName | string | 当前操作人账号或名称 |
| RequestPath | string | 请求路径 |
| IpAddress | string | 客户端 IP |
| UserAgent | string | 客户端 User-Agent |
| CreatedAt | DateTime | 操作时间 |

`ChangeJson` 格式：

```json
{
  "CustomerName": {
    "old": "张三合作社",
    "new": "张三药材合作社"
  },
  "Status": {
    "old": "PENDING",
    "new": "FOLLOWING"
  }
}
```

## ActionType 推断

基础规则：

| EF 状态 | ActionType |
| --- | --- |
| Added | Create |
| Deleted | Delete |
| Modified | 继续按字段推断 |

`Modified` 字段推断规则：

1. 如果变更字段全部属于状态字段，记为 `StatusChange`。
2. 如果变更字段全部属于负责人字段，记为 `AssignOwner`。
3. 其他情况记为 `Update`。

状态字段命名约定：

- `Status`
- `State`
- `Enabled`
- `IsEnabled`
- `IsActive`

负责人字段命名约定：

- `OwnerUserId`
- `AssigneeId`
- `ResponsibleUserId`
- `ManagerUserId`

如果一次修改同时命中状态字段和负责人字段，第一版统一记为 `Update`，避免误判。

## ChangeJson 计算

基于 EF `ChangeTracker` 自动计算：

- `Added`：业务字段记录为 `{ old: null, new: 当前值 }`。
- `Modified`：只记录已修改业务字段，格式为 `{ old: 原值, new: 当前值 }`。
- `Deleted`：业务字段记录为 `{ old: 原值, new: null }`。

过滤字段：

- `Id`
- `CreatedAt`
- `CreatedBy`
- `UpdatedAt`
- `UpdatedBy`
- `IsDeleted`
- `Password`
- `PasswordHash`
- `Token`
- `RefreshToken`

过滤后的 `ChangeJson` 为空时，不写操作日志。

## 实现位置

建议在 `AppDbContext.SaveChanges()` 和 `SaveChangesAsync()` 内统一处理：

1. 从 `ChangeTracker` 收集继承 `BaseEntity` 的新增、修改、删除实体。
2. 排除 `SystemOperationLog` 自身，避免递归记录。
3. 调用现有 `SetAuditFields()`。
4. 根据实体状态和字段变更生成待写入的 `SystemOperationLog`。
5. 将日志加入 `SystemOperationLogs`。
6. 调用 EF 原始保存逻辑。

当前项目实体主键由 `BaseEntity` 构造函数生成 Guid，因此 `Added` 实体在保存前即可取得 `EntityId`。如果未来接入数据库自增主键实体，需要单独处理先保存业务实体再补日志的情况。

## 请求信息

通过 `IHttpContextAccessor` 获取：

- `RequestPath`
- `IpAddress`
- `UserAgent`

`CurrentUserService` 当前已能提供：

- `UserId`
- `Username`

`AppDbContext` 当前只注入 `ICurrentUserService`。实现时可选择：

- 扩展 `ICurrentUserService`，增加请求信息属性；或
- 在 Infrastructure 层新增请求上下文服务，例如 `IRequestContextService`。

推荐第一版扩展 `ICurrentUserService`，改动更小。

## 查询使用

后台审计列表可按以下条件查询：

- `EntityType`
- `EntityId`
- `ActionType`
- `OperatorUserId`
- `OperatorName`
- `RequestPath`
- `CreatedAt` 时间范围

业务详情时间线可按 `EntityType + EntityId` 查询，并由前端或查询 DTO 根据 `ActionType` 和 `ChangeJson` 动态生成展示文案。

## 测试建议

后端测试放在 `CodexTemp` 中，覆盖：

- 新增实体自动生成 `Create` 日志。
- 修改普通字段生成 `Update` 日志。
- 只修改状态字段生成 `StatusChange` 日志。
- 只修改负责人字段生成 `AssignOwner` 日志。
- 删除实体生成 `Delete` 日志。
- 审计字段和敏感字段不会进入 `ChangeJson`。
- `SystemOperationLog` 自身不会递归生成日志。
