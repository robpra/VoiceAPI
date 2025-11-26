using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VoiceAPI.Data;
using VoiceAPI.Hubs;
using VoiceAPI.Models.Auth;
using VoiceAPI.Services;
using VoiceAPI.Utils;   // <── NECESARIO para ServicioHelper

var builder = WebApplication.CreateBuilder(args);

// =================================================
// 1) CONFIGURACIÓN DE JWT
// =================================================
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSettings = jwtSection.Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

// =================================================
// 2) BASE DE DATOS (MySql/MariaDB)
// =================================================
builder.Services.AddDbContext<AgentContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))
    );
});

// =================================================
// 3) INYECCIÓN DE SERVICIOS
// =================================================

// SignalR
builder.Services.AddSignalR();

// JWT Service
builder.Services.AddSingleton<JwtService>();

// ServicioHelper (❗ FALTABA — CAUSABA TU ERROR)
builder.Services.AddSingleton<ServicioHelper>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =================================================
// 4) CORS — REQUERIDO PARA DAVINCI + SIGNALR
// =================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

// =================================================
// 5) AUTH + JWT
// =================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Necesario para JWT en WebSockets (SignalR)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/eventsHub"))
                {
                    context.Token = accessToken; // JWT viene por query
                }

                return Task.CompletedTask;
            }
        };
    });

// =================================================
// BUILD APP
// =================================================
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

// CORS debe ir ANTES de usar controllers y hubs
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Controllers REST
app.MapControllers();

// SignalR WebSocket Hub
app.MapHub<EventsHub>("/eventsHub");

// Run
app.Run();

