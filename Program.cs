using VoiceAPI.Services;
using VoiceAPI.Hubs;
using VoiceAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
//builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddHttpClient();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (COMPATIBLE CON SIGNALR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)    // permite cualquier origen
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();              // NECESARIO PARA WEBSOCKETS
    });
});

// HttpClient
builder.Services.AddHttpClient();

// VoiceAPI Services
builder.Services.AddSingleton<PBXManager>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<AriClient>();
builder.Services.AddSingleton<ExtensionAllocator>();

// SignalR
builder.Services.AddSignalR().AddJsonProtocol();

// Base de Datos

builder.Services.AddDbContext<AgentDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        new MySqlServerVersion(new Version(10, 6)) // MariaDB 10.x
    )
);


var app = builder.Build();

app.UseHttpsRedirection();

// CORS debe ir antes de endpoints
app.UseCors("AllowAll");

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Controllers
app.MapControllers();

// SignalR Hub
app.MapHub<EventsHub>("/eventsHub");

app.Run();

