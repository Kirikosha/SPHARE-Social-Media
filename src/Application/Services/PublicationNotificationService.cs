using Application.Features.Publications.Commands;
using Application.Features.Publications.Queries;
using Application.Features.Users.Queries;
using Application.Services.EmailService;
using Application.Services.SubscriptionService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PublicationNotificationService : BackgroundService
{
    private readonly ILogger<PublicationNotificationService> _logger;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(60);
    private const int BatchSize = 20;

    public PublicationNotificationService(ILogger<PublicationNotificationService> logger, IMediator mediator, 
        IEmailService emailService, ISubscriptionService subscriptionService)
    {
        _logger = logger;
        _mediator = mediator;
        _emailService = emailService;
        _subscriptionService = subscriptionService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Service started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPublicationsAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in notification service main loop");
            }

            await Task.Delay(_checkInterval, cancellationToken);
        }
    }

    private async Task ProcessPublicationsAsync(CancellationToken stoppingToken)
    {
        var followersCache = new Dictionary<string, List<string>>(); // Key - author id, value - list of follower ids
        List<Publication> publicationsBatch;

        var lastDate = DateTime.UtcNow;
        do
        {
            var result = await _mediator.Send(new GetPublicationsToRemind.Query { BatchSize = BatchSize, PostedAt = 
                lastDate});

            if (!result.IsSuccess || result.Value == null)
            {
                throw new Exception("To fix later - PublicationNotificationService.cs, line 62");
            }
            publicationsBatch = result.Value;
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
                lastDate = publicationsBatch[^1].PostedAt;

        } while (publicationsBatch.Count == BatchSize && !stoppingToken.IsCancellationRequested);
    }

    private async Task ProcessSinglePublicationAsync(Publication? publication, List<string> followers, 
        CancellationToken 
            stoppingToken)
    {
        try
        {
            // Reload publication to ensure we have fresh data
            if (publication == null || publication.WasSent) return;

            if (!followers.Any()) return;

            var results = await _mediator.Send(new GetUserEmailsByIds.Query { Ids = followers });
            if (results.Value == null || !results.Value.Any()) return;

            string body = CreateBody(publication, "https://localhost:4200");

            // Send emails in batches
            var batchSize = 10;
            for (int i = 0; i < results.Value.Count; i += batchSize)
            {
                var batch = results.Value.Skip(i).Take(batchSize).ToList();
                await _emailService.SendEmailsAsync(batch, "New publication was published", body);

                if (i + batchSize < results.Value.Count)
                {
                    await Task.Delay(1000, stoppingToken); // Rate limiting
                }
            }

            await _mediator.Send(new SetPublicationSentState.Query { Id = publication.Id, State = true });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing publication {PublicationId}", publication!.Id);
        }
    }

    private async Task<List<string>> GetFollowersIds(string authorId)
    {
        List<string> ids = await _subscriptionService.GetFollowersAsync(authorId);
        return ids;
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
