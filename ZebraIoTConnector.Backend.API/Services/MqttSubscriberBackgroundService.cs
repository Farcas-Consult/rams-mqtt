using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZebraIoTConnector.Client.MQTT.Console.Configuration;
using ZebraIoTConnector.Client.MQTT.Console.Subscriptions;

namespace ZebraIoTConnector.Backend.API.Services
{
    public class MqttSubscriberBackgroundService : BackgroundService
    {
        private readonly ILogger<MqttSubscriberBackgroundService> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public MqttSubscriberBackgroundService(
            ILogger<MqttSubscriberBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("MQTT Subscriber Background Service is starting.");

                // Get MQTT topic from configuration
                var topic = configuration["Mqtt:Topic"] ?? "zebra/#";

                using (var scope = serviceProvider.CreateScope())
                {
                    var subscriptionManager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                    var configurationManager = scope.ServiceProvider.GetRequiredService<ZebraIoTConnector.Client.MQTT.Console.Configuration.IConfigurationManager>();

                    // Subscribe to MQTT topics
                    subscriptionManager.Subscribe(topic);
                    logger.LogInformation($"Subscribed to MQTT topic: {topic}");

                    // Configure readers (download config & operation mode)
                    configurationManager.ConfigureReaders();
                    logger.LogInformation("Reader configuration completed.");
                }

                // Keep the service running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to connect to MQTT broker. The Web API will continue to run, but MQTT features (live feed, tag reading) will be unavailable. Ensure 'mosquitto' is running if you need these features.");
                // Do not throw, so the rest of the app (API) stays alive.
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("MQTT Subscriber Background Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}

