# QPS 项目

QPS 是一个基于 .NET 8 的棋牌室管理系统后端服务，提供房间管理、订单处理、会员管理等核心功能。

## 技术栈

| 分类 | 技术 | 说明 |
|------|------|------|
| 框架 | ASP.NET Core 8.0 | Web 开发框架 |
| 语言 | C# 12 | 编程语言 |
| ORM | Entity Framework Core 8 | 数据访问 |
| 数据库 | SQLite | 轻量级嵌入式数据库 |
| 消息模式 | MediatR | 中介者模式（CQRS） |
| 身份认证 | JWT | JSON Web Token |
| 验证 | FluentValidation | 数据验证框架 |
| API 文档 | Swagger | 交互式 API 文档 |
| IoT 通信 | MQTTnet | MQTT 客户端库 |

## 项目架构

采用经典的分层架构，各层职责清晰：

```
QPS/
├── src/
│   ├── 1.QPS.Domain/             # 领域层 - 核心业务逻辑和实体
│   ├── 2.QPS.Application/        # 应用层 - 业务用例和 DTO
│   ├── 3.QPS.Infrastructure/     # 基础设施层 - 技术实现
│   └── 4.QPS.WebAPI/             # 表现层 - API 接口
├── tests/
│   ├── QPS.UnitTests/            # 单元测试
│   └── QPS.IntegrationTests/     # 集成测试
└── QPS.sln                       # 解决方案文件
```

### 1. QPS.Domain (领域层)

**核心实体**：
- `Merchant` - 商户信息
- `Shop` - 店铺信息
- `Room` - 房间实体（含状态管理）
- `RoomImage` - 房间图片
- `Order` - 订单实体（含生命周期）
- `OrderItem` - 订单项
- `Customer` - 客户信息
- `Coupon` - 优惠券
- `Plan` - 套餐方案
- `Tag` - 标签
- `User` / `Role` / `Permission` - RBAC 权限系统

**公共组件**：
- `BaseEntity` - 实体基类（含 MerchantId）
- `AggregateRoot` - 聚合根基类
- `DomainEvent` - 领域事件基类
- `BusinessException` - 业务异常

### 2. QPS.Application (应用层)

**设计模式**：采用 CQRS（命令查询职责分离）模式

**功能模块**：
| 模块 | 功能 |
|------|------|
| Auth | 用户登录、退出 |
| Roles | 角色管理（增删改查） |
| Users | 用户管理（增删改查） |
| Shops | 店铺管理（增删改查） |
| Rooms | 房间管理（状态控制、电源管理） |
| RoomImages | 房间图片管理 |
| Orders | 订单创建、结算、自动完成 |
| Plans | 套餐管理 |
| Coupons | 优惠券管理 |
| Customers | 客户管理 |
| Tags | 标签管理 |

**管道行为**：
- `ValidationBehaviour` - 请求参数验证
- `TransactionBehaviour` - 事务管理

### 3. QPS.Infrastructure (基础设施层)

**组件**：
- **Database** - 数据库上下文和实体配置
- **Identity** - JWT 令牌生成和当前用户服务
- **IoT** - MQTT 设备通信服务

### 4. QPS.WebAPI (表现层)

**控制器**：
| 控制器 | 路径 | 说明 |
|--------|------|------|
| `AuthController` | `/api/auth` | 认证接口 |
| `RoleController` | `/api/admin/roles` | 角色管理接口 |
| `UserController` | `/api/admin/users` | 用户管理接口 |
| `ShopController` | `/api/admin/shops` | 店铺管理接口 |
| `RoomController` | `/api/admin/rooms` | 房间管理接口 |
| `PlanController` | `/api/admin/plans` | 套餐管理接口 |
| `CouponController` | `/api/admin/coupons` | 优惠券管理接口 |
| `CustomerController` | `/api/admin/customers` | 客户管理接口 |
| `BookingController` | `/api/app/booking` | 订单预订接口 |
| `OrderController` | `/api/admin/orders` | 订单管理接口 |

**特性**：
- 统一响应格式包装
- 全局异常处理
- 自动租户过滤

## 核心特性

### 1. 多租户架构
- 基于 `MerchantId` 的数据隔离
- 全局查询过滤器自动过滤租户数据
- 支持多店铺管理

### 2. 房间状态管理
```csharp
// 房间状态流转
Idle → Occupied → Cleaning → Idle
     ↘ Fault → (维修) → Idle
```

### 3. 订单生命周期
```csharp
// 订单状态流转
WaitingPayment → Paid → Completed
               ↘ Timeout
               ↘ Cancelled
               ↘ Refunding → Refunded
```

### 4. 权限系统 (RBAC)
- 角色：管理员、店长、收银员
- 基于 JWT 的身份认证
- 请求级别权限控制

### 5. 订单自动完成
订单到期后自动完成，恢复房间状态为空闲

## 开发规范

### 命名规范
- 命令类：`CreateXXXCommand`、`UpdateXXXCommand`、`DeleteXXXCommand`
- 查询类：`GetXXXQuery`、`GetXXXsQuery`
- DTO 类：`XXXDto`、`XXXCreateRequest`、`XXXUpdateRequest`

### 代码风格
- 遵循 .NET 官方编码规范
- 使用记录类型（Record）作为 DTO
- 异步优先原则

## 目录结构详解

```
src/
├── 1.QPS.Domain/
│   ├── Common/          # 基类和通用组件
│   ├── Entities/        # 领域实体
│   ├── Events/          # 领域事件
│   └── Exceptions/      # 业务异常
├── 2.QPS.Application/
│   ├── Behaviours/      # 管道行为
│   ├── Common/          # 通用响应包装
│   ├── Contracts/       # DTO 定义
│   ├── Features/        # 功能实现（Command/Query）
│   ├── Interfaces/      # 服务接口
│   ├── Pagination/      # 分页支持
│   └── Validators/      # 验证器
├── 3.QPS.Infrastructure/
│   ├── Database/        # EF Core 配置
│   ├── Identity/        # 认证实现
│   └── IoT/             # MQTT 服务
└── 4.QPS.WebAPI/
    ├── Controllers/     # API 控制器
    ├── Filters/         # 过滤器
    ├── Data/            # 测试数据初始化
    └── Program.cs       # 应用入口
```

## 快速开始

### 环境要求
- .NET 8.0 SDK
- SQLite（已内置）

### 运行项目

```bash
# 克隆项目
git clone <repository-url>
cd QPS-HT

# 构建项目
dotnet build

# 运行 API
cd src/4.QPS.WebAPI
dotnet run
```

### 访问地址
- API 服务：http://localhost:5000
- Swagger 文档：http://localhost:5000/swagger

## License

MIT License