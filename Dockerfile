# 使用 .NET 8.0 SDK 作为构建镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 设置工作目录
WORKDIR /app

# 复制项目文件
COPY . .

# 恢复依赖
RUN dotnet restore

# 构建项目
RUN dotnet build --configuration Release --no-restore

# 发布项目
RUN dotnet publish src/4.QPS.WebAPI --configuration Release --output /app/publish --no-build

# 使用 .NET 8.0 ASP.NET Core Runtime 作为运行镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# 设置工作目录
WORKDIR /app

# 从构建镜像复制发布文件
COPY --from=build /app/publish .

# 暴露端口
EXPOSE 80
EXPOSE 443

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:80

# 启动应用
ENTRYPOINT ["dotnet", "QPS.WebAPI.dll"]