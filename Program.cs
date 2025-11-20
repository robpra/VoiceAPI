using VoiceAPI.Services;
using VoiceAPI.Hubs;
using VoiceAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONTROLADORES + JSON
// ======================================================
builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddHttpClient();

// ======================================================
// SWAGGER
// ======================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ======================================================
// CORS (REQUERIDO PARA SIGNALR + WEBRTC)
// ======================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // NECESARIO PARA WEBSOCKETS
    });
});

// ======================================================
// SERVICIOS DE LA VOICEAPI
// ======================================================
builder.Services.AddSingleton<PBXManager>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<AriClient>();
builder.Services.AddSingleton<ExtensionAllocator>();

// ======================================================
// SIGNALR (CONFIG WEBRTC)
// ======================================================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
})
.AddJsonProtocol();

// ======================================================
// BASE DE DATOS (MariaDB / MySQL)
// ======================================================
builder.Services.AddDbContext<AgentDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        new MySqlServerVersion(new Version(10, 6)) // MariaDB 10.x
    )
);

var app = builder.Build();

// ======================================================
// PIPELINE HTTP
// ======================================================
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// ======================================================
// SIGNALR HUB (ruta real del front)
// ======================================================
app.MapHub<EventsHub>("/eventsHub");

app.Run();

