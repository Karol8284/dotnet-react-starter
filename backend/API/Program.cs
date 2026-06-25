using API.Filters;
using API.Middleware;
using API.Configurations;
using Application.Services;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Shared.Settings;

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
    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    if (!builder.Environment.IsEnvironment("Integration"))
    {
        builder.Services.AddSwaggerGen();
    }

    // Configure DbContext
    var connectionString = builder.Configuration["DefaultConnection"]
        ?? builder.Configuration["DbConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsEnvironment("Integration"))
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("IntegrationTestDb"));
    }
    else
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string not found");
        }

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    // Configure JWT Settings
    builder.Services.AddOptions<JwtSettings>()
        .Bind(builder.Configuration.GetSection("Jwt"))
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret), "JWT Secret is required.")
        .Validate(settings => settings.Secret.Length >= 32, "JWT Secret must be at least 32 characters long.")
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), "JWT Issuer is required.")
        .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), "JWT Audience is required.")
        .Validate(settings => settings.AccessTokenExpiresInMinutes > 0, "AccessTokenExpiresInMinutes must be greater than 0.")
        .Validate(settings => settings.RefreshTokenExpiresInDays > 0, "RefreshTokenExpiresInDays must be greater than 0.")
        .ValidateOnStart(); // Validate on application startup


    builder.Services.AddHttpContextAccessor();
    // Configure JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

    builder.Services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

    // Register services
    builder.Services.AddScoped<ValidationFilterAttribute>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, MockAuthService>(); // Mock for testing (replace with real AuthService later)
   


    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactApp", policy =>
            policy.WithOrigins(
                "http://localhost:5173",// Vite dev server
                "https://localhost:5173",
                "http://localhost:3000" // Create React App / alternatywny port
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials() // Potrzebne gdy używasz HttpOnly cookies w przyszłości
                );
    });

    var app = builder.Build();

    Log.Information("📊 Configuring application middleware...");

    // Apply migrations and seed data automatically
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create database schema from models
            await dbContext.Database.EnsureCreatedAsync();

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
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("📖 Swagger UI available at /swagger");
    }

    app.UseHttpsRedirection();

    // JWT Authentication & Authorization
    app.UseCors("ReactApp");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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
