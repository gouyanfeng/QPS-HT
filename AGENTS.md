# Agent 指南

每次run项目时，先杀掉原来的 5000 端口，再启动项目。


# 测试用的token
authorization
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJiOWZlYjg4Mi05NDgwLTRjNWItOWUxYS05MjUxODU0YzUzZmEiLCJtZXJjaGFudElkIjoiN2I0NDc4ODYtNTM5Yi00MWQxLWJjNzEtZWMxNTljYmU5ZmUwIiwicm9sZSI6IkFkbWluIiwiZXhwIjoxNzc4NjY0ODc1LCJpc3MiOiJRUFMuQVBJIiwiYXVkIjoiUVBTLkNsaWVudCJ9.1v1lq72YwspuK3QqlI1Hbd7kpBcLfHN74qU0NxTRfh0

# 测试用的数据库
4.QPS.WebAPI/QPSChessRoom.db

# 测试用的swagger
http://localhost:5000/swagger/v1/swagger.json

# 本地 IIS 发布

本地 IIS 后端站点固定使用 `http://localhost:20004`，不要随意更换端口。

后端发布目录固定为：

```powershell
E:\Code\QPS\QPS-HT\CodexTemp\publish\QPS.WebAPI
```

发布前先构建验证：

```powershell
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln" --no-restore
dotnet test "E:\Code\QPS\QPS-HT\QPS.sln" --no-build --logger "console;verbosity=minimal"
```

正常发布命令：

```powershell
dotnet publish "E:\Code\QPS\QPS-HT\src\4.QPS.WebAPI\QPS.WebAPI.csproj" -c Release -o "E:\Code\QPS\QPS-HT\CodexTemp\publish\QPS.WebAPI"
```

如果发布时报 `w3wp.exe` 锁定 DLL，不要杀 IIS 进程。先在发布目录放置 `app_offline.htm` 让 IIS 自动卸载应用，等待几秒后重新 publish，发布成功后必须删除 `app_offline.htm` 恢复站点。

```powershell
New-Item -ItemType File -Force -Path "E:\Code\QPS\QPS-HT\CodexTemp\publish\QPS.WebAPI\app_offline.htm" | Out-Null
Start-Sleep -Seconds 3
dotnet publish "E:\Code\QPS\QPS-HT\src\4.QPS.WebAPI\QPS.WebAPI.csproj" -c Release -o "E:\Code\QPS\QPS-HT\CodexTemp\publish\QPS.WebAPI"
Remove-Item -LiteralPath "E:\Code\QPS\QPS-HT\CodexTemp\publish\QPS.WebAPI\app_offline.htm" -Force
```

发布后至少验证：

```powershell
Invoke-WebRequest -Uri "http://localhost:20004/api/admin/crm/herb-bases?page=1&pageSize=1" -UseBasicParsing -TimeoutSec 20
```

前端开发端口固定使用 `http://localhost:5173`，不要为了联调随意改端口。

# 后端当前设计约定

## 通用约定

- 继承 `BaseEntity` 的实体默认走 EF 全局软删除过滤，常规查询不要重复写 `!IsDeleted`，只有必须查全量时才使用 `IgnoreQueryFilters()`。
- 数据库不要为了调试反复清空。需要本地数据时优先增量补齐或使用初始化逻辑。
- 后端开发调试端口仍按 `5000` 处理；本地 IIS 发布验证端口固定是 `20004`。

## CRM 约定

- CRM 客户当前按药材基地模型处理，实体类型统一使用 `CRM_HERB_BASE`，不要再新增或使用 `CRM_CUSTOMER` 作为新业务编码。
- CRM 相关硬编码优先收敛到 `QPS.Application.Features.Crm.CrmCodes`，不要在 handler 里散落 `"CRM_HERB_BASE"`、`"CRM_VENDOR"`、`"CRM_MAIN_PRODUCT"`、`"PENDING"`、`"FOLLOWING"` 等字符串。
- 客户来源字段使用 `SourceId`，不要再使用 `SourceLeadId`。
- 主营品类、采购品类这类实体扩展属性统一走 `CrmBusinessEntityAttributes`，不要再为单个属性新增独立表。
- `CrmBusinessEntityAttributes.EntityType` 使用 `CRM_HERB_BASE`、`CRM_VENDOR` 这类实体编码，`AttributeCode` 使用 `CRM_MAIN_PRODUCT` 等属性编码。
- 枚举类字典编码和值统一使用全大写加下划线，例如 `CRM_HERB_BASE_STATUS_FOLLOWING` / `FOLLOWING`。前端展示用名称，入库和接口传输用编码值。
- `[SystemPermissions]` 里的权限编码也使用全大写加下划线，后端种子和前端权限码要保持一致。
- 客户负责人来源于 `OwnerUserId` 关联 `SystemUsers`，列表和详情需要返回负责人名称时在查询层补齐。

## Application Handler 风格

- `Handle` 尽量只做编排：读取请求、调用校验/查询/领域方法、保存结果。复杂校验、查询、映射、状态同步放到私有方法。
- 构造函数、`Handle`、私有方法都加简短 XML 注释，说明业务意图，不写空泛注释。
- 方法之间保留一个空行；长条件和长参数列表换行，避免一行塞太多逻辑。
- 领域不变量放 Domain 实体或领域方法里，Application 层负责用例编排和跨仓储/跨服务协作。
- 不为了“统一风格”批量改无关 System 文件。只有确实能减少重复、降低风险或让主流程更清楚时再改。

## 本地测试与临时文件

- 临时测试、Playwright 临时 spec、调试脚本和分析产物放到 `E:\Code\QPS\CodexTemp`，不要放进源码目录。
- 后端改动至少跑：

```powershell
dotnet build "E:\Code\QPS\QPS-HT\QPS.sln" --no-restore
dotnet test "E:\Code\QPS\QPS-HT\QPS.sln" --no-build --logger "console;verbosity=minimal"
```

- 前端改动至少跑：

```powershell
npm run build
```
