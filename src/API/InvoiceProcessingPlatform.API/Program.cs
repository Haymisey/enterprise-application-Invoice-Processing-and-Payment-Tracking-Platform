using FluentValidation;
using InvoiceManagement.Infrastructure;
using PaymentTracking.Infrastructure;
using VendorManagement.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using InvoiceProcessingPlatform.API.Middleware;
using Shared.Application.Behaviors;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Invoice Processing Platform API", Version = "v1" });
});

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

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
