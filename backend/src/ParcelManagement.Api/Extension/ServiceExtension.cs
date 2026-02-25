using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.Filter;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.BackgroundServices;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Core.UnitOfWork;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using ParcelManagement.Infrastructure.UnitOfWork;

/**
    extension in c# requirements 
    1. static class 
    2. class constructor receive the type to be extended with 'this' keyword 
    3. static method 
**/

namespace ParcelManagement.Api.Extension
{
    public static class ServiceExtensions
    {
        const int QUEUE_LIMIT = 100;
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();

            services.AddScoped<IParcelRepository, ParcelRepository>();
            services.AddScoped<IParcelService, ParcelService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IResidentUnitRepository, ResidentUnitRepository>();
            services.AddScoped<IResidentUnitService, ResidentUnitService>();

            services.AddScoped<IUserResidentUnitRepository, UserResidentUnitRepository>();
            services.AddScoped<IUserResidentUnitService, UserResidentUnitService>();

            services.AddScoped<ITrackingEventRepository, TrackingEventRepository>();
            services.AddScoped<ITrackingEventService, TrackingEventService>();

            services.AddScoped<ILockerRepository, LockerRepository>();
            services.AddScoped<ILockerService, LockerService>();

            services.AddScoped<INotificationPrefRepository, NotificationPrefRepository>();
            services.AddScoped<INotificationPrefService, NotificationPrefService>();

            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<ISessionService, SessionService>();

            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();

            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<TransactionFilter>();

            services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(QUEUE_LIMIT));

            services.AddSingleton<ParcelOverstayService>(); // concrete singleton
            services.AddHostedService(provider  => provider.GetRequiredService<ParcelOverstayService>());
            services.AddSingleton<IParcelOverstayEnqueuer>(provider 
                => provider.GetRequiredService<ParcelOverstayService>());

            services.AddScoped<AdminDataSeeder>();
            return services;
        }
    }
}