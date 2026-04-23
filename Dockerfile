# --- 构建阶段不变 (SDK 通常比较大，没关系，只在构建时用) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build --configuration Release --no-restore
RUN dotnet publish src/4.QPS.WebAPI --configuration Release --output /app/publish --no-build

# --- 运行阶段：换成 Alpine ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Alpine 镜像默认不包含 ICU (国际化库)，如果你的程序涉及特殊的时间格式或货币转换，需要加上这行：
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "QPS.WebAPI.dll"]