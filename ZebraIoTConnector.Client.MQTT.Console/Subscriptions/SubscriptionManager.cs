using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZebraIoTConnector.Client.MQTT.Console.Model;
using ZebraIoTConnector.Client.MQTT.Console.Services;

namespace ZebraIoTConnector.Client.MQTT.Console.Subscriptions
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ILogger<SubscriptionManager> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IMQTTClientService mqttClientService;
        private readonly IConfiguration? configuration;

        public SubscriptionManager(
            ILogger<SubscriptionManager> logger, 
            IServiceScopeFactory serviceScopeFactory,
            IMQTTClientService mqttClientService)
            : this(logger, serviceScopeFactory, mqttClientService, null)
        {
        }

        public SubscriptionManager(
            ILogger<SubscriptionManager> logger, 
            IServiceScopeFactory serviceScopeFactory,
            IMQTTClientService mqttClientService, 
            IConfiguration? configuration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.mqttClientService = mqttClientService ?? throw new ArgumentNullException(nameof(mqttClientService));
            this.configuration = configuration;
        }

        private void CheckIfConnected()
        {
            if (!mqttClientService.IsConnected)
            {
                // Connect to MQTT broker - use config if available, otherwise default to "mosquitto" for backward compatibility
                var brokerHost = configuration?["Mqtt:BrokerHost"] ?? "mosquitto";
                mqttClientService.Connect(brokerHost).Wait();
                mqttClientService.LogMessagePublished += arg => logger.LogDebug(arg);
                logger.LogInformation("Connected!");
            }
        }

        public void Subscribe(string topic)
        {
            CheckIfConnected();
            // Subscribe to all topics under zebra/#
            mqttClientService.Subscribe(topic).Wait();

            logger.LogInformation($"Successfully subscribed to {topic}");

            // Subscribe to events - create new scope for each message to avoid disposed context
            mqttClientService.ApplicationMessageReceived += async args =>
            {
                try
                {
                    await SubscriptionEventReceived(args);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Unable to manage subscription");
                }
            };

        }

        public async Task SubscriptionEventReceived(SubscriptionEventReceived args)
        {
            // Create a new scope for each message to get fresh DbContext
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var subscriptionEventParser = scope.ServiceProvider.GetRequiredService<ISubscriptionEventParser>();
                
                // Default config:
                // zebra/{myreader}/{topic}
                // e.g.
                // zebra/FX000000/data
                // zebra/FX000000/events
                // zebra/FX000000/ctrl/res

                switch (args.Topic.Split('/').Last())
                {
                    case "data":
                        // AWAIT the async call - scope stays alive until this completes!
                        await subscriptionEventParser.TagDataEventParserAsync(args);
                        break;
                    case "events":
                        subscriptionEventParser.ManagementEventParser(args);
                        break;
                    case "res":
                        subscriptionEventParser.AllTopicsResponseParser(args);
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
