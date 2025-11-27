using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VoiceAPI.Data;
using VoiceAPI.Hubs;
using VoiceAPI.Services;
using VoiceAPI.Utils;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ===============================
// LOGGING
// ===============================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ===============================
// CORS
// ===============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", cors =>
    {
        cors.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===============================
// Controllers / JSON
// ===============================
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ===============================
// Swagger
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===============================
// JWT — FIX DEFINITIVO
// ===============================
var jwtKey = config["JwtSettings:Key"];
var jwtIssuer = config["JwtSettings:Issuer"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
        };

        // Permitir JWT en SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/eventsHub"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

// ===============================
// MariaDB
// ===============================
var connectionString = config.GetConnectionString("AgentsDB");

builder.Services.AddDbContext<AgentContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// ===============================
// SERVICES
// ===============================
builder.Services.AddSingleton<JwtService>();

builder.Services.AddSingleton<VoiceLogger>(); 

builder.Services.AddSignalR();



// NECESARIO PARA HookController
builder.Services.AddHttpClient();


// ===============================
// BUILD APP
// ===============================
var app = builder.Build();

// ===============================
// Initialize()
// ===============================
ServicioHelper.Initialize(config);

// ===============================
// PIPELINE
// ===============================
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EventsHub>("/eventsHub");

app.Run();

