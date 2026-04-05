using Application.Core;
using Application.Helpers;
using Application.Repositories.SpamRepository;
using Application.Repositories.UserActivityLogRepository;
using Application.Services.EmailService;
using Application.Services.PasswordResetService;
using Application.Services.PhotoService;
using Application.Services.TokenService;
using Application.Services.UserActionLogger;
using Application.Services.UserInterestsUpdateService;
using Application.Services.ViolationService;
using Application.Transaction;
using Application.Validators;
using FluentValidation;
using MediatR;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssemblyContaining<Application.Features.Users.Queries.GetUsersList.Handler>());

        // Validators
        services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

        // Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Application Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IViolationService, ViolationService>();
        services.AddScoped<ISpamRepository, SpamRepository>();
        services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
        services.AddScoped(typeof(IUserActionLogger<>), typeof(UserActionLogger<>));

        // Options Pattern
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfiles).Assembly);

        // Caching & Background Jobs
        services.AddMemoryCache();
        services.AddHostedService<UserInterestUpdateJob>();

        return services;
    } 
}