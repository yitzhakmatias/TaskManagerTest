using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManagement.Api.Adapters.Http;
using TaskManagement.Api.Adapters.Persistence;
using TaskManagement.Api.Adapters.Security;
using TaskManagement.Api.Core.Application;
using TaskManagement.Api.Core.Application.Validation;
using TaskManagement.Api.Core.Ports;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=tasks.db";

builder.Services.AddDbContext<TaskDbContext>(options => options.UseSqlite(connectionString));

// Repository + Unit of Work (driven adapters) and the application service
// (driving port implementation). ITaskRepository is deliberately NOT registered
// directly in DI - it is only reachable through IUnitOfWork.Tasks, which keeps
// SaveChanges centralized in one place.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();

// --- Auth (JWT bearer tokens) ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<DemoUserOptions>(builder.Configuration.GetSection(DemoUserOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Missing required '{JwtOptions.SectionName}' configuration section.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "Small REST API for creating, listing, and toggling tasks."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token returned by POST /api/auth/login (no \"Bearer \" prefix needed here)."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Centralized handling for *unexpected* exceptions only; validation and
// not-found results are returned directly by the endpoints below.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
});

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// Lightweight readiness probe - used by scripts/start-dev.* to know when the
// API is actually accepting requests before the frontend is started. Stays
// anonymous so the startup scripts don't need a token just to poll it.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .ExcludeFromDescription();

app.MapGroup("/api/auth")
    .MapAuthFunctions()
    .WithTags("Auth");

app.MapGroup("/api/tasks")
    .MapTaskFunctions()
    .WithTags("Tasks")
    .RequireAuthorization();

// Apply Code-First migrations (and their baked-in seed data) on startup so the
// database is ready with zero manual setup steps.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.Migrate();
}

app.Run();

// Exposed so WebApplicationFactory<Program> can boot this app in integration tests.
public partial class Program
{
}
