using Application.Core;
using Application.Features.Users.Queries;
using Application.Helpers;
using Application.Services.EmailService;
using Application.Services.PasswordResetService;
using Application.Services.PhotoService;
using Application.Services.SubscriptionService;
using Application.Services.TokenService;
using Application.Services.ViolationService;
using Application.Transaction;
using Infrastructure;
using Infrastructure.Neo4j;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Neo4j.Driver;
using Serilog;
using System.Text;

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

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
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

builder.Services.AddSignalR();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
