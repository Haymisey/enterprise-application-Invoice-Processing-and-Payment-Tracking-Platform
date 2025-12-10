using FluentValidation;
using InvoiceManagement.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<InvoiceManagement.Application.Commands.CreateInvoice.CreateInvoiceCommand>();
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<InvoiceManagement.Application.Commands.CreateInvoice.CreateInvoiceCommandValidator>();

// Add Invoice Management module
builder.Services.AddInvoiceManagementInfrastructure(builder.Configuration);

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice Processing Platform API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
