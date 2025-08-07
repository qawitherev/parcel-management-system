using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;


var builder = WebApplication.CreateBuilder(args);

// tell the DI container that we have controller
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

// Dependency Injection 
// THE HOLY GRAIL OF ASP.NET CORE
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddScoped<IParcelRepository, ParcelRepository>();
builder.Services.AddScoped<IParcelService, ParcelService>();

var app = builder.Build();

// match the incoming url with implemented route 
app.UseRouting();

// map the url to relevant method 
app.MapControllers();

app.Run(); 