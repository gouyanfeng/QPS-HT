# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 复制项目文件
COPY *.csproj ./
RUN dotnet restore

# 复制所有源代码
COPY . ./

# 构建应用
RUN dotnet publish -c Release -o out

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 从构建阶段复制构建好的应用
COPY --from=build /app/out ./

# 暴露端口
EXPOSE 80

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:80

# 启动应用
ENTRYPOINT ["dotnet", "QPS-HT.dll"]