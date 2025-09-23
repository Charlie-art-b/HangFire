using Hangfire;
using Hangfire.SqlServer;
using SERVERHANGFIRE.Flows.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configuración
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

// Servicios
builder.Services.AddScoped<IReportJobService, ReportJobService>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>(); // Singleton es mejor para Kafka
builder.Services.AddScoped<IHttpClientService, HttpClientService>();

// HttpClient configurado específicamente para el servicio
builder.Services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Hangfire
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(
              builder.Configuration.GetConnectionString("AdventureWorks"),
              new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true
              }
          )
);

builder.Services.AddHangfireServer();

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseAuthorization();

// Configuración de endpoints
app.MapControllers();
app.UseHangfireDashboard("/hangfire");

app.Run();