using Microsoft.EntityFrameworkCore;
using ZebraIoTConnector.Backend.API;
using ZebraIoTConnector.Backend.API.Services;
using ZebraIoTConnector.Persistence;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configure services
DependencyRegistrar.BuildServiceCollection(builder.Services, configuration);

// Register background service for MQTT subscriptions
builder.Services.AddHostedService<MqttSubscriberBackgroundService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Zebra IoT Connector API",
        Version = "v1",
        Description = "Asset Tracking System API for Zebra RFID IoT Connector"
    });
});

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp",
        builder => builder
                .WithOrigins("http://localhost:3000", "http://169.254.7.130:3000", "http://192.168.0.100:3000") // Localhost and Remote Frontend
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Required for SignalR
});

var app = builder.Build();

// Run database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ZebraDbContext>();
    try
    {
        dbContext.Database.Migrate();
        // Seed the database
        DbInitializer.Initialize(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Enable CORS
app.UseCors("AllowClientApp");

// Configure the HTTP request pipeline
// Configure the HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zebra IoT Connector API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
// }

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map SignalR hub
app.MapHub<ZebraIoTConnector.Backend.API.Hubs.LiveFeedHub>("/hubs/livefeed");

app.Run();

