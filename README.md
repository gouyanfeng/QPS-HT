# QPS-HT

## 项目简介

QPS-HT 是一个基于 .NET 8 的 WebAPI 项目，提供高性能的 HTTP 服务。

## 技术栈

- **框架**: .NET 8.0
- **Web 框架**: ASP.NET Core WebAPI
- **容器化**: Docker
- **CI/CD**: GitHub Actions
- **包管理**: NuGet

## 快速开始

### 本地开发

1. **克隆仓库**
   ```bash
   git clone https://github.com/gouyanfeng/QPS-HT.git
   cd QPS-HT
   ```

2. **安装依赖**
   ```bash
   dotnet restore
   ```

3. **运行项目**
   ```bash
   dotnet run
   ```

4. **访问 API 文档**
   - Swagger 文档: `https://localhost:5001/swagger`

### Docker 部署

#### 本地构建和运行

1. **构建镜像**
   ```bash
   docker build -t qps-ht .
   ```

2. **运行容器**
   ```bash
   docker run -d -p 8080:80 --name qps-ht-container qps-ht
   ```

3. **访问应用**
   - 应用地址: `http://localhost:8080`
   - Swagger 文档: `http://localhost:8080/swagger`

#### 使用 GitHub Container Registry

1. **拉取镜像**
   ```bash
   docker pull ghcr.io/gouyanfeng/qps-ht:latest
   ```

2. **运行容器**
   ```bash
   docker run -d -p 8080:80 --name qps-ht-container ghcr.io/gouyanfeng/qps-ht:latest
   ```

## GitHub Actions 配置

项目使用 GitHub Actions 进行 CI/CD 自动化构建和部署。

### 工作流配置

- **触发方式**: 手动触发 (workflow_dispatch)
- **构建环境**: Ubuntu Latest
- **构建产物**: Docker 镜像
- **镜像存储**: GitHub Container Registry

### 手动触发构建

1. 登录 GitHub 仓库
2. 进入 "Actions" 标签页
3. 选择 "Docker Build and Push" 工作流
4. 点击 "Run workflow" 按钮
5. 填写可选参数（版本和平台）
6. 点击 "Run workflow" 开始构建

## API 文档

项目集成了 Swagger UI 用于 API 文档管理。

- **本地开发**: `https://localhost:5001/swagger`
- **Docker 部署**: `http://localhost:8080/swagger`

## 项目结构

```
QPS-HT/
├── .github/workflows/       # GitHub Actions 工作流配置
│   ├── docker-build.yml     # Docker 构建和推送工作流
├── Properties/              # 项目属性
│   ├── launchSettings.json  # 启动配置
├── bin/                     # 构建输出目录
├── obj/                     # 构建中间文件
├── Dockerfile               # Docker 构建文件
├── Program.cs               # 应用入口
├── QPS-HT.csproj            # 项目文件
├── QPS-HT.http              # HTTP 请求测试文件
├── QPS-HT.sln               # 解决方案文件
├── appsettings.json         # 应用配置
├── appsettings.Development.json  # 开发环境配置
└── README.md                # 项目文档
```

## 配置管理

项目使用 `appsettings.json` 和 `appsettings.Development.json` 进行配置管理。

### 主要配置项

- **Kestrel**: 服务器配置
- **Logging**: 日志配置
- **AllowedHosts**: 允许的主机

## 贡献指南

1. **Fork 仓库**
2. **创建特性分支**
   ```bash
   git checkout -b feature/your-feature
   ```
3. **提交更改**
   ```bash
   git commit -m "Add your feature"
   ```
4. **推送到远程**
   ```bash
   git push origin feature/your-feature
   ```
5. **创建 Pull Request**

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 联系信息

- **项目地址**: [https://github.com/gouyanfeng/QPS-HT](https://github.com/gouyanfeng/QPS-HT)
- **作者**: gouyanfeng



## 部署到 Render
[Render](https://render.com/) 是一个基于 Docker 的容器平台，用于部署和管理容器化应用。

- **Render 应用**: [https://dashboard.render.com/apps/qps-ht](https://dashboard.render.com/)

## 应用地址
- **Render 应用**: [https://qps-ht.onrender.com](https://qps-ht.onrender.com)
