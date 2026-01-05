using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZebraIoTConnector.Backend.API.Hubs;
using ZebraIoTConnector.Client.MQTT.Console;
using ZebraIoTConnector.Client.MQTT.Console.Configuration;
using ZebraIoTConnector.Client.MQTT.Console.Publisher;
using ZebraIoTConnector.Client.MQTT.Console.Services;
using ZebraIoTConnector.Client.MQTT.Console.Subscriptions;
using ZebraIoTConnector.FXReaderInterface;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Services;
using ZebraIoTConnector.Backend.API.Services;
using System.Reflection;

namespace ZebraIoTConnector.Backend.API
{
    public class DependencyRegistrar
    {
        public static void BuildServiceCollection(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging((logBuilder) => logBuilder.SetMinimumLevel(LogLevel.Trace).AddConsole());

            // DAL - Configure DbContext with connection string from configuration
            services.AddDbContext<ZebraDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Business services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEquipmentRegistryService, EquipmentRegistryService>();
            
            services.AddScoped<ITagReadNotifier, SignalRTagReadNotifier>();

            // Register MaterialMovementService (it will now inject ITagReadNotifier automatically)
            services.AddScoped<IMaterialMovementService, MaterialMovementService>();
            
            services.AddScoped<IAssetManagementService, AssetManagementService>();
            services.AddScoped<IGateManagementService, GateManagementService>();
            services.AddScoped<IReportingService, ReportingService>();

            // Managers
            services.AddScoped<ZebraIoTConnector.Client.MQTT.Console.Configuration.IConfigurationManager, ZebraIoTConnector.Client.MQTT.Console.Configuration.ConfigurationManager>();
            services.AddScoped<IPublisherManager, PublisherManager>();
            services.AddScoped<ISubscriptionEventParser, SubscriptionEventParser>();
            
            // SubscriptionManager with IConfiguration for MQTT broker settings
            services.AddScoped<ISubscriptionManager>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SubscriptionManager>>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var mqttService = sp.GetRequiredService<IMQTTClientService>();
                return new SubscriptionManager(logger, scopeFactory, mqttService, configuration);
            });

            // Reader interface
            services.AddScoped<IFXReaderManager, FXReaderManager>();

            // MQTT Client Service
            services.AddSingleton<IMQTTClientService, MQTTClientService>();

            // ASP.NET Core services
            services.AddControllers();
            services.AddSignalR();
        }
    }
}

