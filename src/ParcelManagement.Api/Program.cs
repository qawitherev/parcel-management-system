using Microsoft.EntityFrameworkCore;
using ParcelManagement.Api.Middleware;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;


var builder = WebApplication.CreateBuilder(args);

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
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
        builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseMySql(connectionString, serverVersion));
}
builder.Services.AddScoped<IParcelRepository, ParcelRepository>();
builder.Services.AddScoped<IParcelService, ParcelService>();

var app = builder.Build();

// match the incoming url with implemented route 
app.UseRouting();

// map the url to relevant method 
app.MapControllers();

//for swagger - only available in dev 
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddelware>();

app.Run();

namespace ParcelManagement.Api
{
    public partial class Program { }
}