using API.Filters;
using API.Middleware;
using API.Configurations;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Shared.Settings;
using System.Linq;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 7
    )
    .CreateLogger();

try
{
    Log.Information("🚀 Application starting up...");

    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers()
        .AddFluentValidation(config =>
        {
            config.RegisterValidatorsFromAssemblyContaining<Program>();
            config.DisableDataAnnotationsValidation = false;
        });
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks();

    // Configure DbContext
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["DbConnectionString"]
        ?? throw new InvalidOperationException("Connection string not found");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

    // Add Swagger
    builder.Services.AddSwaggerGen();

    // Configure JWT Settings
    builder.Services.AddOptions<JwtSettings>()
        .Bind(builder.Configuration.GetSection("Jwt"))
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret), "JWT Secret is required.")
        .Validate(settings => settings.Secret.Length >= 32, "JWT Secret must be at least 32 characters long.")
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), "JWT Issuer is required.")
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), "JWT Audience is required.")
        .Validate(settings => settings.AccessTokenExpiresInMinutes > 0, "AccessTokenExpiresInMinutes must be greater than 0.")
        .Validate(settings => settings.RefreshTokenExpiresInDays > 0, "RefreshTokenExpiresInDays must be greater than 0.")
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.RefreshTokenCookieName), "RefreshTokenCookieName is required.")
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.RefreshTokenCookiePath), "RefreshTokenCookiePath is required.")
        .Validate(settings => Enum.TryParse<SameSiteMode>(settings.RefreshTokenCookieSameSite, true, out _), "RefreshTokenCookieSameSite must be one of: Strict, Lax, None, Unspecified.")
        .Validate(settings => Enum.TryParse<CookieSecurePolicy>(settings.RefreshTokenCookieSecurePolicy, true, out _), "RefreshTokenCookieSecurePolicy must be one of: Always, SameAsRequest, None.")
        .Validate(settings => !string.Equals(settings.RefreshTokenCookieSameSite, "None", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(settings.RefreshTokenCookieSecurePolicy, "None", StringComparison.OrdinalIgnoreCase),
            "Refresh token cookies with SameSite=None must not use CookieSecurePolicy=None.")
        .ValidateOnStart(); // Validate on application startup

    builder.Services.AddOptions<CorsSettings>()
        .Bind(builder.Configuration.GetSection("Cors"))
        .Validate(settings => settings.AllowedOrigins.Length > 0, "At least one CORS allowed origin is required.")
        .Validate(settings => settings.AllowedOrigins.All(origin => Uri.TryCreate(origin, UriKind.Absolute, out _)),
            "All CORS allowed origins must be absolute URLs.")
        .Validate(settings => !settings.AllowCredentials || settings.AllowedOrigins.All(origin => origin != "*"),
            "Wildcard CORS origins cannot be used when credentials are enabled.")
        .ValidateOnStart();

    var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();


    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHealthChecks();
    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
    if (jwtSettings == null)
        throw new InvalidOperationException("JWT settings not configured in appsettings.json");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

    // Register services
    builder.Services.AddScoped<ValidationFilterAttribute>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, DatabaseAuthService>();
    builder.Services.AddScoped<Application.Interfaces.IUserService, DatabaseUserService>();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });
    });


    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactApp", policy =>
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();

            if (corsSettings.AllowCredentials)
            {
                policy.AllowCredentials();
            }
        });
    });

    var app = builder.Build();

    Log.Information("📊 Configuring application middleware...");

    // Apply migrations and seed data automatically
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply pending Entity Framework migrations
            await dbContext.Database.MigrateAsync();

            Log.Information("✓ Database initialized successfully!");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "✗ Database initialization failed");
        }
    }

    // Configure the HTTP request pipeline
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("📖 Swagger UI available at /swagger");
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowWasm");

    // JWT Authentication & Authorization
    app.UseCors("ReactApp");

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("🌐 Application listening on configured ports");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
}
finally
{
    Log.Information("🛑 Application shutting down...");
    await Log.CloseAndFlushAsync();
}
