using API.Filters;
using API.Middleware;
using Application.Services;
using Domain.Interfaces;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Shared.Settings;
using System.Text;

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

    // Configure DbContext
    var connectionString = builder.Configuration["DefaultConne  ction"]
        ?? builder.Configuration["DbConnectionString"]
        ?? throw new InvalidOperationException("Connection string not found");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Add Swagger
    builder.Services.AddSwaggerGen();

    // Configure JWT Settings
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

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
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<ValidationFilterAttribute>();

    // Register services
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, MockAuthService>(); // Mock for testing (replace with real AuthService later)

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowWasm", policy =>
            policy.WithOrigins("https://localhost:7234")
                .AllowAnyMethod()
                .AllowAnyHeader());
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
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("📖 Swagger UI available at /swagger");
    }

    app.UseHttpsRedirection();

    // JWT Authentication & Authorization
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
