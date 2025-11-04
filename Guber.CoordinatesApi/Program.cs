using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Add Controllers + Swagger
// -------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -------------------------
// Configure CORS
// -------------------------
const string corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy.WithOrigins("http://localhost:5157")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// -------------------------
// Retry Policy for HttpClient
// -------------------------
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => (int)msg.StatusCode == 429)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

// -------------------------
// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing");
Console.WriteLine("[DEBUG] JWT Key (validation): " + jwtKey);


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Guber.LiveTracking",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Guber.LiveTracking"
        };

        //debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("[DEBUG] JWT validation failed: " + context.Exception.Message);
                if (context.Exception is SecurityTokenInvalidSignatureException)
                    Console.WriteLine("[DEBUG] Invalid signature detected.");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("[DEBUG] JWT token validated successfully for user: " +
                    context.Principal?.Identity?.Name ?? context.Principal?.FindFirst("sub")?.Value);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    var token = authHeader.ToString();
                    Console.WriteLine("[DEBUG] Raw Authorization Header: " + token);

                    if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                    {
                        context.Token = token.Substring("Bearer ".Length).Trim();
                        Console.WriteLine("[DEBUG] JWT received: " + context.Token);
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] No valid Bearer token found in Authorization header.");
                    }
                }
                else
                {
                    Console.WriteLine("[DEBUG] Authorization header not found.");
                }

                return Task.CompletedTask;
            }
    };
    });

builder.Services.AddAuthorization();

// -------------------------
// Dependency Injection
// -------------------------
builder.Services.AddSingleton<Guber.CoordinatesApi.Services.ILocationStore, Guber.CoordinatesApi.Services.InMemoryLocationStore>();
builder.Services.AddSingleton<Guber.CoordinatesApi.Services.IFareService, Guber.CoordinatesApi.Services.FareService>();

builder.Services.AddHttpClient<Guber.CoordinatesApi.Services.IGeocodingService, Guber.CoordinatesApi.Services.NominatimGeocodingService>(client =>
{
    var cfg = builder.Configuration.GetSection("Geocoding");
    client.BaseAddress = new Uri(cfg["BaseUrl"] ?? "https://nominatim.openstreetmap.org");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(cfg["UserAgent"] ?? "GuberCoordinatesModule/1.0");
    var email = cfg["Email"];
    if (!string.IsNullOrWhiteSpace(email))
        client.DefaultRequestHeaders.Add("From", email);
    client.Timeout = TimeSpan.FromSeconds(cfg.GetValue("TimeoutSeconds", 10));
}).AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient<Guber.CoordinatesApi.Services.IRoutingService, Guber.CoordinatesApi.Services.OsrmRoutingService>(client =>
{
    var cfg = builder.Configuration.GetSection("Routing");
    client.BaseAddress = new Uri(cfg["BaseUrl"] ?? "https://router.project-osrm.org");
    client.Timeout = TimeSpan.FromSeconds(cfg.GetValue("TimeoutSeconds", 10));
}).AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<Guber.CoordinatesApi.Services.IEstimateService, Guber.CoordinatesApi.Services.EstimateService>();

// -------------------------
// Build the app
// -------------------------
var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
