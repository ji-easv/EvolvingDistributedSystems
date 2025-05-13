using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using UserMicroservice.Infrastructure;
using UserMicroservice.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("UserDb");
    options.UseNpgsql(connectionString);
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(config =>
{
    config.GroupNameFormat = "'v'V";
    config.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.AddUserApi()
    .WithApiVersionSet(apiVersionSet)
    .MapToApiVersion(1);

app.Run();