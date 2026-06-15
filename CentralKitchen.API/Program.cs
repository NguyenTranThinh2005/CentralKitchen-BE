using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CentralKitchen.Application;
using CentralKitchen.Infrastructure;
using CentralKitchen.API.Middlewares;
using CentralKitchen.API.Security;

var builder = WebApplication.CreateBuilder(args);

// Add Layers DI
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();

// Configure Authentication
var jwtSecret = builder.Configuration["Supabase:JwtSecret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("Supabase JWT Secret is missing from configurations.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = !string.IsNullOrEmpty(builder.Configuration["JwtSettings:Issuer"]),
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = !string.IsNullOrEmpty(builder.Configuration["JwtSettings:Audience"]),
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "authenticated",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register Claims Transformation to load Role and StoreId from DB
builder.Services.AddTransient<IClaimsTransformation, SupabaseClaimsTransformation>();

// Configure Policy-based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireStoreStaff", policy => policy.RequireRole("store_staff"));
    options.AddPolicy("RequireKitchenStaff", policy => policy.RequireRole("kitchen_staff"));
    options.AddPolicy("RequireManager", policy => policy.RequireRole("manager"));
    options.AddPolicy("RequireManagerOrKitchenStaff", policy => policy.RequireRole("manager", "kitchen_staff"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Central Kitchen Franchise API",
        Version = "v1",
        Description = "ASP.NET Core Web API for Central Kitchen Franchise Store & Inventory Management system."
    });

    // Support Bearer Token input in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only (do not include the 'Bearer ' prefix, it will be added automatically).\n\nExample: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments to show description in Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    var appXmlPath = Path.Combine(AppContext.BaseDirectory, "CentralKitchen.Application.xml");
    if (File.Exists(appXmlPath))
    {
        options.IncludeXmlComments(appXmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true) // Enable Swagger in all environments for ease of testing
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });
}

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/swagger");
    await Task.CompletedTask;
});

app.Run();
