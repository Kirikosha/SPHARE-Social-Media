using Application.Behaviors;
using Application.Core;
using Application.Features.Users.Commands;
using Application.Interfaces;
using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Application.Services.TokenService;
using Application.Settings;
using Application.Validators;
using Domain.Entities;
using FluentValidation;
using Infrastructure.HostedServices;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using MediatR;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssemblyContaining<Application.Features.Users.Queries.GetUsersList.Handler>());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>(includeInternalTypes:true);
        // Pipeline Behaviors
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Application Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IViolationNotificationService, ViolationNotificationNotificationService>();
        services.AddScoped<ISpamRepository, SpamRepository>();
        services.AddScoped<IViolationService, ViolationService>();
        services.AddScoped<IViolationRepository, ViolationRepository>();
        services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
        services.AddScoped<IUserInterestUpdater, UserInterestUpdateService>();
        services.AddScoped(typeof(IUserActionLogger<>), typeof(UserActionLogger<>));

        services.AddScoped<IPublicationRepository, PublicationRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IComplaintService, ComplaintService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IPublicationService, PublicationService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IMlTaggingService, MlTaggingService>();
        services.AddScoped<IPublicationTagService, PublicationTagService>();
        
        

        // Options
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.Configure<InterestUpdateSettings>(configuration.GetSection("InterestUpdate"));

        services.AddHttpClient<IMlTaggingService, MlTaggingService>(client =>
        {
            client.BaseAddress = new Uri(configuration["MlService:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfiles).Assembly);

        // Caching & Background Jobs
        services.AddMemoryCache();
        services.AddHostedService<UserInterestUpdateJob>();

        return services;
    } 
}