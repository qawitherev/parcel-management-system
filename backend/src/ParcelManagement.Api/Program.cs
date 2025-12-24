using Microsoft.EntityFrameworkCore;
using ParcelManagement.Api.Middleware;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using System.Text.Json.Serialization;
using ParcelManagement.Api.Utility;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Core.UnitOfWork;
using ParcelManagement.Infrastructure.UnitOfWork;
using ParcelManagement.Api.Filter;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ParcelManagement.Api.Swagger;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// while we can do if (... || builder.Environment.IsEnvironment("Testing"))
// we shouldn't do that because testing should have their own config 
// so we init the settings in factory
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
            .AllowAnyMethod();
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


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    var jwtSettings = new JWTSettings();
    builder.Configuration.GetSection("JWTSettings").Bind(jwtSettings);
    JwtBearerConfiguration.JwtBearerOptionsConfig(option, jwtSettings);
    option.Events = JwtBearerAccessEvent.UnauthorizedOrForbiddenAccessEvent();

});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IParcelRepository, ParcelRepository>();
builder.Services.AddScoped<IParcelService, ParcelService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IResidentUnitRepository, ResidentUnitRepository>();
builder.Services.AddScoped<IResidentUnitService, ResidentUnitService>();

builder.Services.AddScoped<IUserResidentUnitRepository, UserResidentUnitRepository>();
builder.Services.AddScoped<IUserResidentUnitService, UserResidentUnitService>();

builder.Services.AddScoped<ITrackingEventRepository, TrackingEventRepository>();
builder.Services.AddScoped<ITrackingEventService, TrackingEventService>();

builder.Services.AddScoped<ILockerRepository, LockerRepository>();
builder.Services.AddScoped<ILockerService, LockerService>();

builder.Services.AddScoped<INotificationPrefRepository, NotificationPrefRepository>();
builder.Services.AddScoped<INotificationPrefService, NotificationPrefService>();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<TransactionFilter>();

builder.Services.AddScoped<AdminDataSeeder>();

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

// search all route defined 
app.UseRouting();

// apply CORS
app.UseCors("Allow-Angular-FrontEnd");

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

app.UseMiddleware<ApiExceptionMiddelware>();

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