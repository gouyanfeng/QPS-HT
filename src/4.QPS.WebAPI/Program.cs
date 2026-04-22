using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;
using QPS.Application.Behaviours;
using QPS.Application.Interfaces;
using QPS.Infrastructure.IoT;
using QPS.Infrastructure.Identity;
using QPS.Infrastructure.Database;
using QPS.WebAPI.Data;
using QPS.WebAPI.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // 注册响应包装过滤器
    options.Filters.Add<ResponseWrapperFilter>();
})
.AddJsonOptions(options =>
{
    // 配置 JSON 序列化选项，使用驼峰命名
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QPS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
    });
});

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// Register application services
builder.Services.AddScoped<IDbContext, AppDbContext>();
builder.Services.AddScoped<IMqttService, MqttClientService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtGenerator>(_ => new JwtGenerator(
    builder.Configuration["Jwt:SecretKey"],
    builder.Configuration["Jwt:Issuer"],
    builder.Configuration["Jwt:Audience"]
));

// Services are now directly implemented in handlers

// Register MediatR and behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(QPS.Application.Features.Rooms.GetRoomsHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    // Initialize test data
    TestDataInitializer.Initialize(dbContext);
}

// Configure the HTTP request pipeline.
// 启用 Swagger 在所有环境中
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
// 提取异常处理逻辑为方法
app.UseExceptionHandler(HandleException);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Response wrapper is now handled by ResponseWrapperFilter

app.MapControllers();

app.Run();

// 异常处理方法
static void HandleException(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async context =>
    {

        Console.WriteLine($"HandleException:");

        var exceptionPayload = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionPayload?.Error;

        context.Response.StatusCode = 200; // 依然返回 200，让前端 Axios 走成功回调
        context.Response.ContentType = "application/json";

        // 逻辑：根据异常类型决定 code
        int errorCode = 500;
        string message = "服务器内部错误";

        // 如果是你自定义的业务异常，可以提取具体的错误码和消息
        if (exception is QPS.Domain.Exceptions.BusinessException bizEx)
        {
            errorCode = bizEx.ErrorCode;
            message = bizEx.Message;
        }

        var response = QPS.Application.Common.ApiResponse<object>.Fail(errorCode, message);

        await context.Response.WriteAsJsonAsync(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    });
}