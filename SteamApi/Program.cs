using ClickHouse.Client.ADO;
using Microsoft.EntityFrameworkCore;
using SteamApi.Infrastructure;
using SteamApi.Application.Services;
using SteamApi.Infrastructure.Background;
using SteamApi.Infrastructure.JsonConverters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Добавляем поддержку DateOnly
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL (EF Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// ClickHouse connection
builder.Services.AddSingleton(provider =>
{
    var chConnString = builder.Configuration.GetConnectionString("ClickHouse");
    return new ClickHouseConnection(chConnString);
});

// HttpClient for Steam
builder.Services.AddHttpClient();

// Application services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ISteamSyncService, SteamSyncService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IClickHouseWriter, ClickHouseWriter>();
builder.Services.AddScoped<IApiFacade, ApiFacade>();
builder.Services.AddHostedService<SyncBackgroundService>();

// JWT Authentication (плюс по ТЗ)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-super-secret-key-that-is-at-least-32-characters-long")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// DB init with migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    
    // Initialize ClickHouse schema (temporarily disabled)
    try
    {
        var chWriter = scope.ServiceProvider.GetRequiredService<IClickHouseWriter>();
        await chWriter.EnsureSchemaAsync(CancellationToken.None);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ClickHouse initialization failed: {ex.Message}");
        // Continue without ClickHouse
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<SteamApi.Infrastructure.Middleware.ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
