using Application.Features.Users.Queries;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Application.Core;
using MediatR;
using Application.Transaction;
using Application.Services.EmailService;
using Application.Services.PasswordResetService;
using Application.Services.PhotoService;
using Application.Services.SubscriptionService;
using Application.Services.TokenService;
using Application.Services.ViolationService;
using Application.Helpers;
using Infrastructure.Neo4j;
using Neo4j.Driver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultPGConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblyContaining<GetUsersList.Handler>();
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
builder.Services.AddScoped<IEmailService, EmailService>(); // SMTP Settings
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IPhotoService, PhotoService>(); // Cloudinary settings
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IViolationService, ViolationService>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

var settings = new Neo4jSettings();
builder.Configuration.GetSection("Neo4jSettings").Bind(settings);
try
{
    var neo4jDriver = GraphDatabase.Driver(
        settings.Neo4jConnection,
        AuthTokens.Basic(settings.Neo4juser, settings.Neo4jPassword)
    );
    builder.Services.AddSingleton(neo4jDriver);
    builder.Services.AddTransient<INeo4jDataAccess, Neo4jDataAccess>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to initialize Neo4j Driver");
}
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
