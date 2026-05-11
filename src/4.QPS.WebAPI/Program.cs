using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;

using QPS.Application.Behaviours;
using QPS.Application.Features.Orders;
using QPS.Application.Interfaces;
using QPS.Infrastructure;
using QPS.Infrastructure.IoT;
using QPS.Infrastructure.Identity;
using QPS.Infrastructure.Database;
using QPS.WebAPI.Data;
using QPS.WebAPI.Filters;
using System.Text;

/// <summary>
/// 程序入口类
/// </summary>
var builder = WebApplication.CreateBuilder(args);

#region 服务注册

/// <summary>
/// 添加控制器服务
/// </summary>
builder.Services.AddControllers(options =>
{
    // 注册响应包装过滤器，统一处理API响应格式
    options.Filters.Add<ResponseWrapperFilter>();
})
.AddJsonOptions(options =>
{
    // 配置 JSON 序列化选项，使用驼峰命名
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    // 配置 JSON 编码器，允许特殊字符
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

/// <summary>
/// 配置跨域（CORS）
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // 允许所有来源
               .AllowAnyMethod()  // 允许所有HTTP方法
               .AllowAnyHeader(); // 允许所有HTTP头
    });
});

/// <summary>
/// 配置 Swagger/OpenAPI 文档
/// </summary>
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 配置API文档信息
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QPS API", Version = "v1" });

    // 配置JWT认证方案
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,          // 令牌位置：请求头
        Description = "Please enter token",     // 描述信息
        Name = "Authorization",                // 头名称
        Type = SecuritySchemeType.Http,         // 安全方案类型：HTTP
        BearerFormat = "JWT",                  // 令牌格式：JWT
        Scheme = "bearer"                      // 认证方案：bearer
    });

    // 添加安全要求
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
    });
});

/// <summary>
/// 配置数据库
/// </summary>
builder.Services.AddDbContext<AppDbContext>(options =>
    // 使用SQLite数据库，连接字符串从配置中获取
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

/// <summary>
/// 配置认证
/// </summary>
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 配置JWT令牌验证参数
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,                // 验证颁发者
            ValidateAudience = true,              // 验证受众
            ValidateLifetime = true,              // 验证过期时间
            ValidateIssuerSigningKey = true,      // 验证签名密钥
            ValidIssuer = builder.Configuration["Jwt:Issuer"],       // 有效颁发者
            ValidAudience = builder.Configuration["Jwt:Audience"],   // 有效受众
            // 签名密钥
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

/// <summary>
/// 注册应用服务
/// </summary>
builder.Services.AddHttpContextAccessor(); // 添加HTTP上下文访问器

// 添加基础设施服务（数据库、身份认证、MQTT等）
builder.Services.AddInfrastructure();

// 注册JWT生成器
builder.Services.AddScoped<IJwtGenerator>(_ => new JwtGenerator(
    builder.Configuration["Jwt:SecretKey"],  // 密钥
    builder.Configuration["Jwt:Issuer"],    // 颁发者
    builder.Configuration["Jwt:Audience"]   // 受众
));

/// <summary>
/// 注册MediatR和行为
/// </summary>
builder.Services.AddMediatR(cfg =>
{
    // 从应用程序程序集注册服务，使用程序集名称
    var assembly = System.Reflection.Assembly.Load("QPS.Application");
    cfg.RegisterServicesFromAssembly(assembly);

    // 添加验证行为（执行顺序1）
    // 用于验证请求参数的合法性
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

    // 添加事务行为（执行顺序2）
    // 用于确保操作的原子性，自动处理事务的提交和回滚
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
});



#endregion

/// <summary>
/// 构建应用程序
/// </summary>
var app = builder.Build();

#region 数据库初始化

/// <summary>
/// 初始化数据库
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // 确保数据库已创建
    dbContext.Database.EnsureCreated();

    // 初始化测试数据
    TestDataInitializer.Initialize(dbContext);
}

#endregion

#region HTTP请求管道配置

/// <summary>
/// 配置HTTP请求管道
/// </summary>
// 启用Swagger文档（在所有环境中）
app.UseSwagger();
app.UseSwaggerUI();

// 启用跨域
app.UseCors("AllowAll");

// 启用异常处理
app.UseExceptionHandler(HandleException);

// 启用HTTPS重定向
app.UseHttpsRedirection();

// 启用认证
app.UseAuthentication();

// 启用授权
app.UseAuthorization();



// 映射控制器路由
app.MapControllers();

#endregion

/// <summary>
/// 运行应用程序
/// </summary>
app.Run();

#region 异常处理方法

/// <summary>
/// 异常处理方法
/// </summary>
/// <param name="appBuilder">应用程序构建器</param>
static void HandleException(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async context =>
    {
        // 记录异常信息
        Console.WriteLine($"HandleException:");

        // 获取异常信息
        var exceptionPayload = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionPayload?.Error;

        // 设置响应状态码为200，让前端Axios走成功回调
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";

        // 默认错误码和消息
        int errorCode = 500;
        string message = "服务器内部错误";

        // 处理自定义业务异常
        if (exception is QPS.Domain.Exceptions.BusinessException bizEx)
        {
            errorCode = bizEx.ErrorCode;
            message = bizEx.Message;
        }

        // 创建错误响应
        var response = QPS.Application.Common.ApiResponse<object>.Fail(errorCode, message);

        // 序列化响应并写入HTTP响应
        await context.Response.WriteAsJsonAsync(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    });
}

#endregion