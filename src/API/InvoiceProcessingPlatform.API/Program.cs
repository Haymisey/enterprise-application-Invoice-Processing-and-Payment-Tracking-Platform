using FluentValidation;
using System.Security.Claims;
using System.Text.Json;
using InvoiceManagement.Infrastructure;
using InvoiceManagement.Infrastructure.Persistence;
using PaymentTracking.Infrastructure;
using PaymentTracking.Infrastructure.Persistence;
using VendorManagement.Infrastructure;
using VendorManagement.Infrastructure.Persistence;
using AIClassification.Infrastructure;
using AIClassification.Infrastructure.Persistence;
using Reporting.Infrastructure;
using Reporting.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using InvoiceProcessingPlatform.API.Middleware;
using Shared.Application.Behaviors;
using Shared.Application.Messaging;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Application.Commands.CreateInvoice;
using PaymentTracking.Application.Commands.SchedulePayment;
using VendorManagement.Application.Commands.RegisterVendor;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Invoice Processing Platform API",
        Version = "v1",
        Description = "API for Invoice Processing and Payment Tracking Platform"
    });
    
    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer YOUR_TOKEN_HERE'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add MediatR - register handlers from all modules
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(InvoiceManagement.Application.Commands.CreateInvoice.CreateInvoiceCommand).Assembly,
        typeof(PaymentTracking.Application.Commands.SchedulePayment.SchedulePaymentCommand).Assembly,
        typeof(VendorManagement.Application.Commands.RegisterVendor.RegisterVendorCommand).Assembly,
        typeof(AIClassification.Application.Commands.ClassifyInvoice.ClassifyInvoiceCommand).Assembly
    );
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<InvoiceManagement.Application.Commands.CreateInvoice.CreateInvoiceCommandValidator>();

// Add modules in order - last registered IUnitOfWork will be used by default
// Each module's handlers should use their specific DbContext
// Add Invoice Management module
builder.Services.AddInvoiceManagementInfrastructure(builder.Configuration);

// Add Vendor Management module  
builder.Services.AddVendorManagementInfrastructure(builder.Configuration);

// Add Payment Tracking module LAST - this ensures PaymentTracking handlers get PaymentDbContext
builder.Services.AddPaymentTrackingInfrastructure(builder.Configuration);

// Add AI Classification module
builder.Services.AddAIClassificationInfrastructure(builder.Configuration);

// Add Reporting module
builder.Services.AddReportingInfrastructure(builder.Configuration);

// Add JWT Authentication with Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakConfig = builder.Configuration.GetSection("Keycloak");
        
        options.Authority = keycloakConfig["Authority"];
        options.Audience = keycloakConfig["Audience"];
        options.RequireHttpsMetadata = keycloakConfig.GetValue<bool>("RequireHttpsMetadata");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = keycloakConfig.GetValue<bool>("ValidateIssuer"),
            ValidateAudience = keycloakConfig.GetValue<bool>("ValidateAudience"),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "preferred_username",
            RoleClaimType = "role" // Use "role" as the claim type for roles
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully");

                if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                {
                    // Inspect all claims (for debugging)
                    foreach (var claim in claimsIdentity.Claims)
                    {
                        Console.WriteLine($"🔍 Claim: {claim.Type} = {claim.Value}");
                    }

                    // Map Keycloak realm_access roles
                    var realmAccessClaim = context.Principal.FindFirst("realm_access");
                    if (realmAccessClaim != null)
                    {
                        try 
                        {
                            using var doc = JsonDocument.Parse(realmAccessClaim.Value);
                            if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleValue = role.GetString();
                                    if (!string.IsNullOrEmpty(roleValue))
                                    {
                                        // Use "role" to match RoleClaimType set in TokenValidationParameters
                                        claimsIdentity.AddClaim(new Claim("role", roleValue));
                                        Console.WriteLine($"🎭 Mapped Keycloak role: {roleValue}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Error parsing realm_access roles: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Warning: 'realm_access' claim not found in token.");
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register Event Bus (RabbitMQ)
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new EventBus(configuration);
});

// Register Outbox Processor (Background Worker)
builder.Services.AddHostedService<OutboxProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice Processing Platform API v1");
        c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });

    // Initialize Databases
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var invoiceContext = services.GetRequiredService<InvoiceDbContext>();
            await invoiceContext.Database.MigrateAsync();
            
            var paymentContext = services.GetRequiredService<PaymentDbContext>();
            await paymentContext.Database.MigrateAsync();
            
            var vendorContext = services.GetRequiredService<VendorDbContext>();
            await vendorContext.Database.MigrateAsync();

            var classificationContext = services.GetRequiredService<ClassificationDbContext>();
            await classificationContext.Database.MigrateAsync();

            var reportingContext = services.GetRequiredService<ReportingDbContext>();
            await reportingContext.Database.MigrateAsync();
            
            Console.WriteLine("✅ Databases migrated and updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during database migration: {ex.Message}");
        }
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
