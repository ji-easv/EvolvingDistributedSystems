﻿using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using UserMicroservice.Application;
using UserMicroservice.Infrastructure;
using UserMicroservice.Presentation.Apis;
using UserMicroservice.Presentation.Middleware;

namespace UserMicroservice;

public class Startup(IConfiguration configuration)
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("UserDb");
            options.UseNpgsql(connectionString);
        });

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(options => { options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0; });
        services.AddSwaggerGen();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddApiVersioning(options =>
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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();
        app.UseExceptionHandler();
        app.UseRouting();
        
        app.UseEndpoints(endpoints =>
        {
            var apiVersionSet = endpoints.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();
            
            endpoints.AddUserApi()
                .WithApiVersionSet(apiVersionSet)
                .MapToApiVersion(1);
            
            if (env.IsDevelopment())
            {
                endpoints.MapOpenApi();
            }
        });

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}