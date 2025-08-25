using Microsoft.EntityFrameworkCore;
using ParcelManagement.Api.Middleware;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// while we can do if (... || builder.Environment.IsEnvironment("Testing"))
// we shouldn't do that because testing should have their own config 
// so we init the settings in factory
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// tell the DI container that we have controller
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection 
// THE HOLY GRAIL OF ASP.NET CORE
if (!builder.Environment.IsEnvironment("Testing")) // --> if we're not doing integration testing, connect to real mySQL, else dbContext is created in CustomWebApplicationFactory.cs
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseMySql(connectionString, serverVersion));
}

// config jwt to builder.service
var jwtSettings = new JWTSettings();
builder.Configuration.GetSection("JWTSettings").Bind(jwtSettings);
if (jwtSettings.SecretKey == null && !builder.Environment.IsEnvironment("Testing"))
{
    throw new InvalidOperationException("Failed to bind JWTSettings from configuration. Please check your appsettings.json or environment variables.");
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    JwtBearerConfiguration.JwtBearerOptionsConfig(option, jwtSettings);
    
});

builder.Services.AddScoped<IParcelRepository, ParcelRepository>();
builder.Services.AddScoped<IParcelService, ParcelService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<JWTSettings>(
    builder.Configuration.GetSection("JWTSettings")
);
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IResidentUnitRepository, ResidentUnitRepository>();
builder.Services.AddScoped<IResidentUnitService, ResidentUnitService>();

builder.Services.AddScoped<IUserResidentUnitRepository, UserResidentUnitRepository>();
builder.Services.AddScoped<IUserResidentUnitService, UserResidentUnitService>();

builder.Services.AddScoped<ITrackingEventRepository, TrackingEventRepository>();



var app = builder.Build();

// search all route defined 
app.UseRouting();

// authentication to populate HttpContext.User
app.UseAuthentication();
// check if populated HttpContext.User is legit 
app.UseAuthorization();

// map the received route with the controller and execute it 
app.MapControllers();


//for swagger - only available in dev 
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddelware>();

app.Run();

// exposing the program class so webApplicationFactory can use it 
namespace ParcelManagement.Api
{
    public partial class Program { }
}