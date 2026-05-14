using Microsoft.EntityFrameworkCore;
using ParcelManagement.Api.Middleware;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ParcelManagement.Infrastructure.Database;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.Filter;
using Microsoft.OpenApi.Models;
using ParcelManagement.Api.Swagger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ParcelManagement.Core.Model.Configuration;
using ParcelManagement.Api.Extension;

var root = Directory.GetCurrentDirectory();
DotNetEnv.Env.Load(Path.Combine(root, ".env"));

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// tell the DI container that we have controller
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TransactionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Parcel API version 1", Version = "v1" });
    option.SwaggerDoc("v2", new OpenApiInfo { Title = "Parcel API version 2", Version = "v2" });

    option.DocInclusionPredicate(SwaggerSetup.DocInclusionPredicate);

    option.OperationFilter<SwaggerVersionParameterRemover>();
    option.DocumentFilter<ReplaceVersionWithValue>();
});
builder.Services.AddControllers().AddJsonOptions(
    options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
);
builder.Services.AddApiVersioning(option =>
{
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.ReportApiVersions = true;
});

// CORS 
builder.Services.AddCors(options =>
{   
    options.AddPolicy("Allow-Angular-FrontEnd", policy =>
    {   // we'll change the origin later 
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Dependency Injection 
// THE HOLY GRAIL OF ASP.NET CORE
if (!builder.Environment.IsEnvironment("Testing")) // --> if we're not doing integration testing, connect to real mySQL, else dbContext is created in CustomWebApplicationFactory.cs
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("ConnectionString not found");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseMySql(connectionString, serverVersion));
}

// todo a separate class to register IOptions<T> into DI container 
builder.Services.Configure<JWTSettings>(
    builder.Configuration.GetSection("JWTSettings")
);
builder.Services.Configure<SystemAdmin>(
    builder.Configuration.GetSection("Admin")
);

builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisSettings")
);

builder.Services.Configure<RateLimitSettings>(
    builder.Configuration.GetSection("RateLimitSettings")
);

builder.Services.ConfigureOptions<RateLimiterConfiguration>();
builder.Services.AddRateLimiter();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    var jwtSettings = new JWTSettings();
    builder.Configuration.GetSection("JWTSettings").Bind(jwtSettings);
    JwtBearerConfiguration.JwtBearerOptionsConfig(option, jwtSettings);
    option.Events = JwtBearerAccessEvent.UnauthorizedOrForbiddenAccessEvent();

});

builder.Services.AddHttpContextAccessor();

// register application services 
builder.Services.AddApplicationServices();

if (builder.Environment.EnvironmentName != "Testing")
{
    // health check services registration and configuration 
    builder.Services.AddHealthChecks()
    .AddCheck("check environment secrets", () =>
    {
        var healthData = new Dictionary<string, object>();
        var issues = new List<string>();
        var secretKey = builder.Configuration["JWTSettings:SecretKey"];
        var adminPassword = builder.Configuration["Admin:Password"];
        healthData["SecretKeyExist"] = !string.IsNullOrEmpty(secretKey);
        healthData["AdminPasswordExist"] = !string.IsNullOrEmpty(adminPassword);
        if (string.IsNullOrEmpty(secretKey))
        {
            issues.Add("JWT Secret key is missing");
        }
        if (string.IsNullOrEmpty(adminPassword))
        {
            issues.Add("Admin password is missing");
        }
        if (issues.Count > 0)
        {
            Console.WriteLine("Environment secrets check failed");
            return HealthCheckResult.Unhealthy(
                $"Some environment secrets are missing. {string.Join(", ", issues)}",
                data: healthData
            );
        } else
        {
            Console.WriteLine("Environment secrets check passed");
            return HealthCheckResult.Healthy(
                "All environment secrets are loaded",
                data: healthData
            );
        }
    })
    .AddMySql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "Database health check",
        failureStatus: HealthStatus.Unhealthy
    );

}

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddelware>();

// search all route defined 
app.UseRouting();

// apply CORS
app.UseCors("Allow-Angular-FrontEnd");

// use registered rate limit 
app.UseRateLimiter();

// authentication to populate HttpContext.User
app.UseAuthentication();
// check if populated HttpContext.User is legit 
app.UseAuthorization();

// map the received route with the controller and execute it 
app.MapControllers();

if (builder.Environment.EnvironmentName != "Testing")
{
    app.MapHealthChecks("/health");
}

//for swagger - only available in dev 
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint("/swagger/v1/swagger.json", "Parcel API v1");
        option.SwaggerEndpoint("/swagger/v2/swagger.json", "Parcel API v2");
    });
}

app.UseMiddleware<BlacklistCheckMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AdminDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();

// exposing the program class so webApplicationFactory can use it 
namespace ParcelManagement.Api
{
    public partial class Program { }
}