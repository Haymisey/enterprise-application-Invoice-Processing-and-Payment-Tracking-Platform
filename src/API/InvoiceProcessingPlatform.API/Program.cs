using FluentValidation;
using InvoiceManagement.Infrastructure;
using InvoiceManagement.Infrastructure.Persistence;
using PaymentTracking.Infrastructure;
using PaymentTracking.Infrastructure.Persistence;
using VendorManagement.Infrastructure;
using VendorManagement.Infrastructure.Persistence;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add MediatR - register handlers from all modules
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(InvoiceManagement.Application.Commands.CreateInvoice.CreateInvoiceCommand).Assembly,
        typeof(PaymentTracking.Application.Commands.SchedulePayment.SchedulePaymentCommand).Assembly,
        typeof(VendorManagement.Application.Commands.RegisterVendor.RegisterVendorCommand).Assembly
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
            ClockSkew = TimeSpan.Zero
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
                Console.WriteLine("Token validated successfully");
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
    app.MapOpenApi();
    
    // Serve Swagger UI from a static HTML file
    app.MapGet("/swagger", async context =>
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Invoice Processing Platform API - Swagger UI</title>
                <link rel="stylesheet" type="text/css" href="https://unpkg.com/swagger-ui-dist@5.10.5/swagger-ui.css" />
            </head>
            <body>
                <div id="swagger-ui"></div>
                <script src="https://unpkg.com/swagger-ui-dist@5.10.5/swagger-ui-bundle.js"></script>
                <script>
                    window.onload = function() {
                        SwaggerUIBundle({
                            url: "/openapi/v1.json",
                            dom_id: "#swagger-ui",
                            presets: [
                                SwaggerUIBundle.presets.apis,
                                SwaggerUIBundle.presets.standalone
                            ]
                        });
                    };
                </script>
            </body>
            </html>
            """;
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
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
