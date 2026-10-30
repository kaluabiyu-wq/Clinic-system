using System.Text.Json.Serialization;
using Clinic.Api.Middleware;
using Clinic.Application.Abstractions;
using Clinic.Application.Services;
using Clinic.Application.Validators;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Persistence.Repositories;
using Clinic.Infrastructure.Time;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();

// Application
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientValidator>();

// API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Send and accept enums as strings, e.g. "Male" instead of 1
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

var app = builder.Build();

// Must be first so it can catch exceptions from everything after it
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // /openapi/v1.json
    app.MapScalarApiReference();      // /scalar/v1
}

app.UseHttpsRedirection();

// app.UseAuthentication();           // add this before UseAuthorization once JWT is configured
app.UseAuthorization();

app.MapControllers();

app.Run();
