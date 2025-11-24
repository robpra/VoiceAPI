using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VoiceAPI.Data;
using VoiceAPI.Hubs;
using VoiceAPI.Models.Auth;
using VoiceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// =============================
// 1) HTTPS configurado en 5001
// =============================
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(
            "/etc/pki/tls/certs/pbx.ryd.pfx",
            "1234"
        );
    });
});


//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5001, listenOptions =>
//    {
//        listenOptions.UseHttps(
//            "/etc/pki/tls/certs/pbx.ryd.crt",
//            "/etc/ssl/private/pbx.ryd.key"
//        );
//    });
//});



// =============================
// 2) CONFIG: JwtSettings
// =============================
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSettings = jwtSection.Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

// =============================
// 3) DB: MySQL / MariaDB
// =============================
builder.Services.AddDbContext<AgentContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))
    );
});

// =============================
// 4) AUTH + JWT
// =============================
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

        // NECESARIO PARA JWT en WebSockets (SignalR)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/eventsHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// =============================
// 5) SignalR + Services
// =============================
builder.Services.AddSignalR();
builder.Services.AddSingleton<JwtService>();

// =============================
// 6) Controllers
// =============================
builder.Services.AddControllers();

// =============================
// 7) Swagger
// =============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================
// 8) CORS — FIX REAL PARA SIGNALR
// =============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true)  // <---- CLAVE PARA WS
            .AllowCredentials();
    });
});

// =============================
// BUILD APP
// =============================
var app = builder.Build();

// =============================
// Middleware
// =============================
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

// CORS — DEBE IR ANTES DE AUTH/CONTROLLERS/HUBS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Hub WebSocket
app.MapHub<EventsHub>("/eventsHub");

// RUN
app.Run();

