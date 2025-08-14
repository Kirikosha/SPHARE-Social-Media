namespace Application.Services;

using Application.Features.Publications.Commands;
using Application.Features.Publications.Queries;
using Application.Features.Users.Queries;
using Application.Services.EmailService;
using Application.Services.SubscriptionService;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class PublicationNotificationService : BackgroundService
{
    private readonly ILogger<PublicationNotificationService> logger;
    private readonly IMediator mediator;
    private readonly IEmailService emailService;
    private readonly ISubscriptionService subscriptionService;
    private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(60);
    private const int BatchSize = 20;

    public PublicationNotificationService(IServiceProvider services, ILogger<PublicationNotificationService> logger, IMediator mediator, 
        IEmailService emailService, ISubscriptionService subscriptionService)
    {
        this.logger = logger;
        this.mediator = mediator;
        this.emailService = emailService;
        this.subscriptionService = subscriptionService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Notification Service started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPublicationsAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in notification service main loop");
            }

            await Task.Delay(checkInterval, cancellationToken);
        }
    }

    private async Task ProcessPublicationsAsync(CancellationToken stoppingToken)
    {
        var followersCache = new Dictionary<int, List<int>>(); // Key - author id, value - list of follower ids
        List<Publication> publicationsBatch;

        int lastId = 0;
        do
        {
            publicationsBatch = await mediator.Send(new GetPublicationsToRemind.Query { BatchSize = BatchSize, LastId = lastId });

            foreach (var publication in publicationsBatch)
            {
                if (!followersCache.TryGetValue(publication.AuthorId, out var followers))
                {
                    followers = await GetFollowersIds(publication.AuthorId);
                    followersCache[publication.AuthorId] = followers;
                }
                await ProcessSinglePublicationAsync(publication, followers, stoppingToken);
            }

            if (publicationsBatch.Count > 0)
                lastId = publicationsBatch[^1].Id;

        } while (publicationsBatch.Count == BatchSize && !stoppingToken.IsCancellationRequested);
    }

    private async Task ProcessSinglePublicationAsync(Publication publication, List<int> followers, CancellationToken stoppingToken)
    {
        try
        {
            // Reload publication to ensure we have fresh data
            if (publication == null || publication.WasSent) return;

            if (!followers.Any()) return;

            var emails = await mediator.Send(new GetUserEmailsByIds.Query { Ids = followers });
            if (!emails.Any()) return;

            string body = CreateBody(publication, "https://localhost:4200");

            // Send emails in batches
            var batchSize = 10;
            for (int i = 0; i < emails.Count; i += batchSize)
            {
                var batch = emails.Skip(i).Take(batchSize).ToList();
                await emailService.SendEmailsAsync(batch, "New publication was published", body);

                if (i + batchSize < emails.Count)
                {
                    await Task.Delay(1000, stoppingToken); // Rate limiting
                }
            }

            await mediator.Send(new SetPublicationSentState.Query { Id = publication.Id, State = true });

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing publication {PublicationId}", publication.Id);
        }
    }

    private async Task<List<int>> GetFollowersIds(int authorId)
    {
        List<int> ids = await subscriptionService.GetFollowersAsync(authorId);
        return ids ?? new List<int>();
    }


    private static string CreateBody(Publication publication, string appBaseUrl)
    {
        var author = publication.Author;
        var hasImages = publication.Images != null && publication.Images.Any();

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <!-- CSS styles from above -->
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>New Publication from {author.Username}</h2>
        </div>
        
        <div class='content'>
            <p>Hello,</p>
            <p>{author.Username} has just published something new that you might find interesting:</p>
            
            <div class='publication-card'>
                <div class='author-info'>
                    <img src='{author.ProfileImage?.ImageUrl ?? "https://example.com/default-profile.png"}' 
                         alt='{author.Username}' class='profile-image'>
                    <div>
                        <div class='author-name'>{author.Username}</div>
                        <div class='timestamp'>Posted at: {publication.PostedAt.ToString("MMMM dd, yyyy h:mm tt")}</div>
                    </div>
                </div>
                
                <div class='publication-content'>
                    {publication.Content}
                </div>
                
                {(hasImages ? $@"
                <img src='{publication.Images!.First().ImageUrl}' 
                     alt='Publication image' class='publication-image'>
                " : "")}
                
                <div style='margin-top: 15px;'>
                    <a href='{appBaseUrl}/publications/{publication.Id}' class='button'>View Publication</a>
                </div>
            </div>
            
            <p style='margin-top: 25px;'>
                You're receiving this notification because you're following {author.Username}. 
            </p>
        </div>
        
        <div class='footer'>
            <a href='{appBaseUrl}'>Visit our website</a>
        </div>
    </div>
</body>
</html>";

        return body;
    }
}
