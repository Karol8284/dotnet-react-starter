var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS - zezwól frontendu na komunikację
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
}

// Health check endpoint - Docker healthcheck
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Use CORS
app.UseCors("AllowFrontend");

// Routing
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
