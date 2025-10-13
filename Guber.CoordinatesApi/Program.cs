using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (adjust to your UI origin)
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(corsPolicy, p =>
        p.WithOrigins(builder.Configuration.GetSection("Cors:AllowOrigins").Get<string[]>() ?? [])
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// HttpClient resilience (simple retry for transient errors)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => (int)msg.StatusCode == 429) // Too Many Requests
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

// Services (DI)
builder.Services.AddSingleton<Guber.CoordinatesApi.Services.ILocationStore, Guber.CoordinatesApi.Services.InMemoryLocationStore>();
builder.Services.AddSingleton<Guber.CoordinatesApi.Services.IFareService, Guber.CoordinatesApi.Services.FareService>();

builder.Services.AddHttpClient<Guber.CoordinatesApi.Services.IGeocodingService, Guber.CoordinatesApi.Services.NominatimGeocodingService>(client =>
{
    var cfg = builder.Configuration.GetSection("Geocoding");
    var baseUrl = cfg["BaseUrl"] ?? "https://nominatim.openstreetmap.org";
    client.BaseAddress = new Uri(baseUrl);
    var ua = cfg["UserAgent"] ?? "GuberCoordinatesModule/1.0";
    client.DefaultRequestHeaders.UserAgent.ParseAdd(ua);
    var email = cfg["Email"];
    if (!string.IsNullOrWhiteSpace(email))
        client.DefaultRequestHeaders.Add("From", email);
    client.Timeout = TimeSpan.FromSeconds(cfg.GetValue("TimeoutSeconds", 10));
}).AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<Guber.CoordinatesApi.Services.IEstimateService, Guber.CoordinatesApi.Services.EstimateService>();

builder.Services.AddHttpClient<Guber.CoordinatesApi.Services.IRoutingService, Guber.CoordinatesApi.Services.OsrmRoutingService>(client =>
{
    var cfg = builder.Configuration.GetSection("Routing");
    var baseUrl = cfg["BaseUrl"] ?? "https://router.project-osrm.org";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(cfg.GetValue("TimeoutSeconds", 10));
}).AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Make the implicit Program class visible to test projects
public partial class Program { }
