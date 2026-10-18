using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Business;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Database;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;
using HRM_SlowGasMotorFactory_Controllers;
using HRM_SlowGasMotorFactory_Controllers.Clients;
using HRM_SlowGasMotorFactory_Controllers.Jobs;
using HRM_SlowGasMotorFactory_Database.Implementations;
using HRM_SlowGasMotorFactory_Database.Migrations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Logging.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger());

// Register database and business logic services (from previous labs)
builder.Services.AddSingleton<ManualMigration>();
builder.Services.AddTransient<IPostDatabase, PostDatabase>();
builder.Services.AddTransient<IWorkerDatabase, WorkerDatabase>();
builder.Services.AddTransient<IPassMarksDatabase, PassMarksDatabase>();
builder.Services.AddTransient<IPostBusinessLogic, PostBusinessLogic>();
builder.Services.AddTransient<IWorkerBusinessLogic, WorkerBusinessLogic>();
builder.Services.AddTransient<IPassMarksBusinessLogic, PassMarksBusinessLogic>();

// ==================== 5 ЛАБОРАТОРНАЯ РАБОТА (расчёт зарплаты) ====================
builder.Services.AddTransient<IWorkerSalaryDatabase, WorkerSalaryDatabase>();
builder.Services.AddTransient<IWorkerSalaryBusinessLogic, WorkerSalaryBusinessLogic>();

// HTTP client for SalesInfoClient (to get sales data from CRM)
builder.Services.AddHttpClient("SaleClientName", client =>
{
    var baseAddress = builder.Configuration["SaleApiSettings:BaseAddress"];
    client.BaseAddress = new Uri(baseAddress!);

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "hrm_app"), new Claim(ClaimTypes.Name, "hrm_app") };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SaleApiSettings:SecretKey"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: builder.Configuration["SaleApiSettings:Issuer"]!,
        audience: builder.Configuration["SaleApiSettings:Audience"]!,
        claims: claims,
        expires: DateTime.Now.AddMinutes(60),
        signingCredentials: credentials);

    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {new JwtSecurityTokenHandler().WriteToken(token)}");
});

// Register SalesInfoClient
builder.Services.AddTransient<ISalesInfoClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient("SaleClientName");
    return new SalesInfoClient(client);
});

// Quartz setup for background job (calculate salary every month on the 1st day at 00:00)
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("WorkerSalaryJob");
    q.AddJob<WorkerSalaryJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("WorkerSalaryJob-trigger")
        .WithCronSchedule("0 0 0 1 * ?")); // Cron: seconds minutes hours day-of-month month day-of-week
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
// =====================================================================

// Swagger with authentication
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("BasicAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Basic Authentication"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "BasicAuth"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddOpenApi();

// Authentication
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null)
    .AddCookie();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<ManualMigration>();
    migrator.ApplyMigrations();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRM API V1");
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }