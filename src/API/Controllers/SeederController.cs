using API.Seeders;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class SeederController(ApplicationDbContext context) : BaseApiController
{
    [HttpGet]
    public async Task SeedEverything()
    {
        RecommendationSeeder seeder = new RecommendationSeeder(context);
        await seeder.SeedAsync();
    }
}