using Domain.Entities;
using Domain.Entities.Publications;
using Domain.Entities.RecomendationEntities;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Seeders;

public class RecommendationSeeder(ApplicationDbContext context)
{
    // -----------------------------------------------------------------------
    // Fixed IDs so the seeder is idempotent — run it multiple times safely
    // -----------------------------------------------------------------------

    #region User IDs
    private const string AliceId  = "seed-user-alice-0001";
    private const string BobId    = "seed-user-bob-0002";
    private const string CarlaId  = "seed-user-carla-0003";
    private const string DanId    = "seed-user-dan-0004";
    #endregion

    #region Tag IDs
    private const int TagPhotography = 1;
    private const int TagTravel      = 2;
    private const int TagCooking     = 3;
    private const int TagTech        = 4;
    private const int TagGaming      = 5;
    private const int TagFitness     = 6;
    private const int TagMusic       = 7;
    private const int TagNature      = 8;
    #endregion

    #region Publication IDs
    // Photography / Nature
    private const string Pub01 = "seed-pub-0001"; // photography + nature
    private const string Pub02 = "seed-pub-0002"; // photography
    private const string Pub03 = "seed-pub-0003"; // photography + travel
    // Travel
    private const string Pub04 = "seed-pub-0004"; // travel
    private const string Pub05 = "seed-pub-0005"; // travel + nature
    // Cooking
    private const string Pub06 = "seed-pub-0006"; // cooking
    private const string Pub07 = "seed-pub-0007"; // cooking
    // Tech
    private const string Pub08 = "seed-pub-0008"; // tech
    private const string Pub09 = "seed-pub-0009"; // tech + gaming
    // Gaming
    private const string Pub10 = "seed-pub-0010"; // gaming
    // Fitness
    private const string Pub11 = "seed-pub-0011"; // fitness
    private const string Pub12 = "seed-pub-0012"; // fitness + nature
    // Music
    private const string Pub13 = "seed-pub-0013"; // music
    private const string Pub14 = "seed-pub-0014"; // music + tech
    // Mixed — should surface for different users
    private const string Pub15 = "seed-pub-0015"; // travel + cooking
    #endregion

    public async Task SeedAsync()
    {
        await SeedTagsAsync();
        await SeedUsersAsync();
        await SeedPublicationsAsync();
        await SeedPublicationTagsAsync();
        await SeedActionLogsAsync();       // raw events the background job will read
        await SeedPublicationViewsAsync();
        await SeedUserInterestTagsAsync(); // pre-built weights so recommendations work immediately
        await context.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // Tags
    // -----------------------------------------------------------------------
    private async Task SeedTagsAsync()
    {
        if (await context.Tags.AnyAsync(t => t.Id == TagPhotography)) return;

        context.Tags.AddRange(
            new Tag { Id = TagPhotography, Name = "photography", Category = "Arts"      },
            new Tag { Id = TagTravel,      Name = "travel",       Category = "Lifestyle" },
            new Tag { Id = TagCooking,     Name = "cooking",      Category = "Lifestyle" },
            new Tag { Id = TagTech,        Name = "tech",         Category = "Tech"      },
            new Tag { Id = TagGaming,      Name = "gaming",       Category = "Tech"      },
            new Tag { Id = TagFitness,     Name = "fitness",      Category = "Health"    },
            new Tag { Id = TagMusic,       Name = "music",        Category = "Arts"      },
            new Tag { Id = TagNature,      Name = "nature",       Category = "Lifestyle" }
        );
    }

    // -----------------------------------------------------------------------
    // Users
    // Alice  — strong photography + travel interest
    // Bob    — strong tech + gaming interest
    // Carla  — cooking + fitness interest
    // Dan    — no interactions yet (tests fallback recommendations)
    // -----------------------------------------------------------------------
    private async Task SeedUsersAsync()
    {
        if (await context.Users.AnyAsync(u => u.Id == AliceId)) return;

        var users = new List<User>
        {
            new()
            {
                Id                    = AliceId,
                Username              = "alice_seed",
                UniqueNameIdentifier  = "alice_seed#0001",
                Email                 = "alice@seed.test",
                Role                  = Roles.User,
            },
            new()
            {
                Id                    = BobId,
                Username              = "bob_seed",
                UniqueNameIdentifier  = "bob_seed#0002",
                Email                 = "bob@seed.test",
                Role                  = Roles.User,
            },
            new()
            {
                Id                    = CarlaId,
                Username              = "carla_seed",
                UniqueNameIdentifier  = "carla_seed#0003",
                Email                 = "carla@seed.test",
                Role                  = Roles.User,
            },
            new()
            {
                Id                    = DanId,
                Username              = "dan_seed",
                UniqueNameIdentifier  = "dan_seed#0004",
                Email                 = "dan@seed.test",
                Role                  = Roles.User,
            }
        };

        context.Users.AddRange(users);

        // SpamRating is required — seed one per user
        context.SpamRatings.AddRange(users.Select(u => new SpamRating
        {
            UserId = u.Id
        }));
    }

    // -----------------------------------------------------------------------
    // Publications  (authored by Alice and Bob so Dan/Carla get recommendations
    // from people they haven't interacted with)
    // -----------------------------------------------------------------------
    private async Task SeedPublicationsAsync()
    {
        if (await context.Publications.AnyAsync(p => p.Id == Pub01)) return;

        var publications = new[]
        {
            Pub("Golden hour at the cliffs — my best shot this year",             Pub01, AliceId, daysAgo: 2,  views: 320),
            Pub("Street photography tips for beginners",                           Pub02, AliceId, daysAgo: 5,  views: 210),
            Pub("Capturing movement while travelling light",                       Pub03, AliceId, daysAgo: 8,  views: 180),
            Pub("10 hidden gems in South-East Asia you must visit",                Pub04, BobId,   daysAgo: 1,  views: 540),
            Pub("Wild camping in Patagonia — a photo diary",                       Pub05, BobId,   daysAgo: 3,  views: 430),
            Pub("One-pan pasta recipe that actually tastes great",                 Pub06, CarlaId, daysAgo: 4,  views: 290),
            Pub("Meal prepping for the week in under an hour",                     Pub07, CarlaId, daysAgo: 6,  views: 175),
            Pub("Why I switched from VS Code to Neovim",                           Pub08, BobId,   daysAgo: 2,  views: 610),
            Pub("Building a game engine from scratch in C++",                      Pub09, BobId,   daysAgo: 7,  views: 390),
            Pub("Elden Ring lore deep-dive — the Elden Beast explained",           Pub10, BobId,   daysAgo: 3,  views: 720),
            Pub("My morning workout routine — 30 minutes, no equipment",           Pub11, CarlaId, daysAgo: 1,  views: 260),
            Pub("Trail running in the Alps — gear list and lessons learned",       Pub12, CarlaId, daysAgo: 9,  views: 195),
            Pub("How I learned jazz piano in six months",                          Pub13, AliceId, daysAgo: 5,  views: 310),
            Pub("Generative music with code — using Tone.js",                     Pub14, BobId,   daysAgo: 4,  views: 275),
            Pub("Thai street food recipes you can make at home after travelling",  Pub15, AliceId, daysAgo: 2,  views: 340),
        };

        context.Publications.AddRange(publications);
    }

    private static Publication Pub(string content, string id, string authorId, int daysAgo, int views) =>
        new()
        {
            Id              = id,
            AuthorId        = authorId,
            Content         = content,
            PostedAt        = DateTime.UtcNow.AddDays(-daysAgo),
            ViewCount       = views,
            PublicationType = PublicationTypes.ordinary,
            IsDeleted       = false
        };

    // -----------------------------------------------------------------------
    // Publication ↔ Tag mappings
    // -----------------------------------------------------------------------
    private async Task SeedPublicationTagsAsync()
    {
        if (await context.PublicationTags.AnyAsync(pt => pt.PublicationId == Pub01)) return;

        var tags = new[]
        {
            PT(Pub01, TagPhotography), PT(Pub01, TagNature),
            PT(Pub02, TagPhotography),
            PT(Pub03, TagPhotography), PT(Pub03, TagTravel),
            PT(Pub04, TagTravel),
            PT(Pub05, TagTravel),      PT(Pub05, TagNature),
            PT(Pub06, TagCooking),
            PT(Pub07, TagCooking),
            PT(Pub08, TagTech),
            PT(Pub09, TagTech),        PT(Pub09, TagGaming),
            PT(Pub10, TagGaming),
            PT(Pub11, TagFitness),
            PT(Pub12, TagFitness),     PT(Pub12, TagNature),
            PT(Pub13, TagMusic),
            PT(Pub14, TagMusic),       PT(Pub14, TagTech),
            PT(Pub15, TagTravel),      PT(Pub15, TagCooking),
        };

        context.PublicationTags.AddRange(tags);
    }

    private static PublicationTag PT(string pubId, int tagId) =>
        new() { PublicationId = pubId, TagId = tagId, IsAutoGenerated = false };

    // -----------------------------------------------------------------------
    // Action Logs
    // These are what the background job reads. Use the enum value name as
    // the Action string to match your UserLogAction enum.
    //
    // Alice  liked/commented on photography, travel, nature content
    // Bob    liked/commented on tech, gaming content
    // Carla  liked/commented on cooking, fitness content
    // Dan    no logs — will hit the fallback recommendation path
    // -----------------------------------------------------------------------
    private async Task SeedActionLogsAsync()
    {
        if (await context.UserLogs.AnyAsync(l => l.UserId == AliceId)) return;

        var logs = new List<UserActionLog>();

        // Alice — photography + travel + nature
        logs.AddRange(new[]
        {
            Log(AliceId, "LikePublication",    Pub01, hoursAgo: 1),
            Log(AliceId, "LikePublication",    Pub02, hoursAgo: 2),
            Log(AliceId, "LikePublication",    Pub03, hoursAgo: 3),
            Log(AliceId, "CreateComment",      Pub01, hoursAgo: 1),
            Log(AliceId, "LikePublication",    Pub04, hoursAgo: 5),
            Log(AliceId, "LikePublication",    Pub05, hoursAgo: 6),
            Log(AliceId, "CreateComment",      Pub04, hoursAgo: 5),
            // A couple of dislikes to test negative signal
            Log(AliceId, "DislikePublication", Pub10, hoursAgo: 4), // gaming — Alice doesn't care
            Log(AliceId, "DislikePublication", Pub09, hoursAgo: 4),
        });

        // Bob — tech + gaming
        logs.AddRange(new[]
        {
            Log(BobId, "LikePublication",    Pub08, hoursAgo: 2),
            Log(BobId, "LikePublication",    Pub09, hoursAgo: 3),
            Log(BobId, "LikePublication",    Pub10, hoursAgo: 1),
            Log(BobId, "CreateComment",      Pub10, hoursAgo: 1),
            Log(BobId, "CreateComment",      Pub08, hoursAgo: 2),
            Log(BobId, "LikePublication",    Pub14, hoursAgo: 4),
            Log(BobId, "DislikePublication", Pub06, hoursAgo: 3), // cooking — Bob doesn't care
        });

        // Carla — cooking + fitness
        logs.AddRange(new[]
        {
            Log(CarlaId, "LikePublication", Pub06, hoursAgo: 2),
            Log(CarlaId, "LikePublication", Pub07, hoursAgo: 3),
            Log(CarlaId, "CreateComment",   Pub06, hoursAgo: 2),
            Log(CarlaId, "LikePublication", Pub11, hoursAgo: 1),
            Log(CarlaId, "LikePublication", Pub12, hoursAgo: 4),
            Log(CarlaId, "CreateComment",   Pub11, hoursAgo: 1),
            Log(CarlaId, "LikePublication", Pub15, hoursAgo: 5), // travel+cooking crossover
        });

        context.UserLogs.AddRange(logs);
    }

    private static UserActionLog Log(string userId, string action, string targetId, int hoursAgo) =>
        new()
        {
            Id                    = Guid.NewGuid().ToString(),
            UserId                = userId,
            Action                = action,
            TargetId              = targetId,
            AdditionalDescription = $"{{\"info\":\"{action} by {userId} on {targetId}\"}}",
            ExecutedAt            = DateTime.UtcNow.AddHours(-hoursAgo)
        };

    // -----------------------------------------------------------------------
    // Publication Views  (separate from likes — also feeds the interest job)
    // -----------------------------------------------------------------------
    private async Task SeedPublicationViewsAsync()
    {
        if (await context.PublicationViews.AnyAsync(v => v.UserId == AliceId)) return;

        var views = new List<PublicationView>
        {
            // Alice views photography + travel content, spends real time on them
            View(AliceId, Pub01, dwellSeconds: 45, hoursAgo: 1),
            View(AliceId, Pub02, dwellSeconds: 30, hoursAgo: 2),
            View(AliceId, Pub03, dwellSeconds: 60, hoursAgo: 3),
            View(AliceId, Pub04, dwellSeconds: 50, hoursAgo: 5),
            View(AliceId, Pub05, dwellSeconds: 40, hoursAgo: 6),
            // Alice briefly skimmed gaming posts (low dwell — weak signal)
            View(AliceId, Pub10, dwellSeconds: 5,  hoursAgo: 4),
            View(AliceId, Pub09, dwellSeconds: 4,  hoursAgo: 4),

            // Bob views tech + gaming content
            View(BobId, Pub08, dwellSeconds: 90, hoursAgo: 2),
            View(BobId, Pub09, dwellSeconds: 75, hoursAgo: 3),
            View(BobId, Pub10, dwellSeconds: 50, hoursAgo: 1),
            View(BobId, Pub14, dwellSeconds: 35, hoursAgo: 4),
            View(BobId, Pub06, dwellSeconds: 3,  hoursAgo: 3), // barely read cooking post

            // Carla views cooking + fitness
            View(CarlaId, Pub06, dwellSeconds: 55, hoursAgo: 2),
            View(CarlaId, Pub07, dwellSeconds: 40, hoursAgo: 3),
            View(CarlaId, Pub11, dwellSeconds: 60, hoursAgo: 1),
            View(CarlaId, Pub12, dwellSeconds: 45, hoursAgo: 4),
            View(CarlaId, Pub15, dwellSeconds: 30, hoursAgo: 5),
        };

        context.PublicationViews.AddRange(views);
    }

    private static PublicationView View(
        string userId, string publicationId, int dwellSeconds, int hoursAgo) =>
        new()
        {
            Id            = Guid.NewGuid().ToString(),
            UserId        = userId,
            PublicationId = publicationId,
            DwellSeconds  = dwellSeconds,
            ViewedAt      = DateTime.UtcNow.AddHours(-hoursAgo)
        };

    // -----------------------------------------------------------------------
    // User Interest Tags (pre-built so recommendations work without waiting
    // for the background job to run first)
    //
    // Weights reflect the engagement pattern above:
    //   Alice  → photography 0.87, travel 0.75, nature 0.60
    //   Bob    → tech 0.85, gaming 0.90, music 0.40
    //   Carla  → cooking 0.82, fitness 0.78, travel 0.30
    //   Dan    → nothing (tests fallback)
    // -----------------------------------------------------------------------
    private async Task SeedUserInterestTagsAsync()
    {
        if (await context.UserInterestTags.AnyAsync(i => i.UserId == AliceId)) return;

        var interests = new[]
        {
            // Alice
            Interest(AliceId, TagPhotography, 0.87f),
            Interest(AliceId, TagTravel,      0.75f),
            Interest(AliceId, TagNature,      0.60f),
            Interest(AliceId, TagGaming,      0.04f), // nearly pruned — negative signals

            // Bob
            Interest(BobId, TagGaming, 0.90f),
            Interest(BobId, TagTech,   0.85f),
            Interest(BobId, TagMusic,  0.40f),

            // Carla
            Interest(CarlaId, TagCooking, 0.82f),
            Interest(CarlaId, TagFitness, 0.78f),
            Interest(CarlaId, TagTravel,  0.30f),
            Interest(CarlaId, TagNature,  0.25f),
        };

        context.UserInterestTags.AddRange(interests);
    }

    private static UserInterestTag Interest(string userId, int tagId, float weight) =>
        new()
        {
            UserId      = userId,
            TagId       = tagId,
            Weight      = weight,
            LastUpdated = DateTime.UtcNow
        };
}