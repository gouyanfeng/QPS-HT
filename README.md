# QPS 项目

## 项目结构

```
QPS/
├── src/
│   ├── 1.QPS.Domain/             # 领域层 - 核心业务逻辑
│   │   ├── Aggregates/           # 聚合根 - 业务实体的组合
│   │   │   ├── MerchantAggregate/ # 商户聚合 - 商户信息管理
│   │   │   ├── OrderAggregate/    # 订单聚合 - 订单生命周期管理
│   │   │   └── RoomAggregate/     # 房间聚合 - 房间状态和设备管理
│   │   ├── Common/               # 通用基类 - 基础类和接口
│   │   ├── Events/               # 领域事件 - 业务事件定义
│   │   └── Exceptions/           # 领域异常 - 业务异常定义
│   ├── 2.QPS.Application/        # 应用层 - 业务用例实现
│   │   ├── Behaviours/           # 行为 - 管道行为（验证、事务）
│   │   ├── Contracts/            # 数据传输对象 - API 请求/响应模型
│   │   ├── Features/             # 功能模块 - 具体业务用例实现
│   │   └── Interfaces/           # 接口 - 服务接口定义
│   ├── 3.QPS.Infrastructure/     # 基础设施层 - 技术实现
│   │   ├── BackgroundJobs/       # 后台任务 - 定时任务和延迟任务
│   │   ├── Identity/             # 身份认证 - JWT 令牌管理
│   │   ├── IoT/                  # 物联网 - MQTT 设备通信
│   │   └── Persistence/          # 持久化 - 数据库访问
│   └── 4.QPS.WebAPI/             # 表现层 - API 接口
│       ├── Controllers/          # 控制器 - API 端点实现
│       ├── Middleware/           # 中间件 - 请求处理管道
│       └── Program.cs            # 应用入口 - 服务配置和启动
├── tests/                        # 测试
│   ├── QPS.UnitTests/            # 单元测试 - 测试单个组件
│   └── QPS.IntegrationTests/     # 集成测试 - 测试组件协作
└── QPS.sln                       # 解决方案文件 - 项目组织
```

## 项目结构详细说明

### 1. QPS.Domain (领域层)

**作用**：定义核心业务逻辑和领域模型，是整个应用的核心。

**业务场景**：
- **MerchantAggregate**：管理商户信息，包括店铺设置、联系信息等
- **OrderAggregate**：处理订单的创建、支付、完成等生命周期
- **RoomAggregate**：管理房间状态、设备配置和设备控制
- **Common**：提供基础类如 Entity、AggregateRoot、ValueObject 等
- **Events**：定义领域事件如 OrderPaidEvent、SessionExpiredEvent 等
- **Exceptions**：定义业务异常如 DomainException 等

### 2. QPS.Application (应用层)

**作用**：实现业务用例，协调领域对象完成业务逻辑。

**业务场景**：
- **Behaviours**：实现管道行为，如请求验证、事务管理
- **Contracts**：定义 API 请求和响应的数据结构
- **Features**：
  - **Rooms**：处理房间相关业务，如获取房间列表、控制房间电源
  - **Orders**：处理订单相关业务，如创建订单、结算订单
  - **Tenants**：处理租户相关业务，如设置店铺信息
- **Interfaces**：定义服务接口，如 IDbContext、IMqttService、ITenantService 等

### 3. QPS.Infrastructure (基础设施层)

**作用**：提供技术实现，如数据库访问、消息通信、身份认证等。

**业务场景**：
- **BackgroundJobs**：实现后台任务，如定时关闭设备电源
- **Identity**：实现身份认证，如 JWT 令牌生成和验证
- **IoT**：实现设备通信，如 MQTT 消息发送和接收
- **Persistence**：实现数据持久化，如数据库连接、实体配置

### 4. QPS.WebAPI (表现层)

**作用**：提供 API 接口，处理 HTTP 请求和响应。

**业务场景**：
- **Controllers**：
  - **Admin**：管理后台接口，如房间管理
  - **App**：客户端应用接口，如订单管理
  - **Auth**：认证相关接口，如登录
- **Middleware**：
  - **TenantIdentification**：租户识别中间件
  - **GlobalExceptionHandler**：全局异常处理中间件
- **Program.cs**：配置服务和启动应用

### 5. 测试项目

**作用**：确保代码质量和功能正确性。

**业务场景**：
- **QPS.UnitTests**：测试单个组件的功能
- **QPS.IntegrationTests**：测试组件之间的协作

## 技术栈

### 核心技术
- **.NET 8.0** - 主要开发框架
- **C#** - 编程语言

### 数据访问
- **Entity Framework Core** - ORM 框架
- **SQL Server** - 数据库

### 消息传递
- **MediatR** - 中介者模式实现
- **MQTTnet** - MQTT 客户端库

### 身份认证
- **JWT** - JSON Web Token



### API 文档
- **Swagger** - API 文档生成

### 验证
- **FluentValidation** - 数据验证

### 其他
- **ASP.NET Core** - Web 框架
- **Dependency Injection** - 依赖注入

## 运行项目

1. **构建解决方案**
   ```bash
   dotnet build
   ```

2. **运行 WebAPI 项目**
   ```bash
   dotnet run --project src/4.QPS.WebAPI
   ```

3. **访问 API 文档**
   - 打开浏览器，访问 `http://localhost:5000/swagger`

## 主要功能

- **房间管理** - 查看房间列表、控制房间电源
- **订单管理** - 创建订单、结算订单
- **租户管理** - 设置店铺信息
- **认证** - JWT 令牌认证
- **IoT 集成** - MQTT 协议控制设备