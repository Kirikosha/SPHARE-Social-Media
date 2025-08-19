namespace API.Controllers;

using Application.Features.Publications.Queries;
using Application.Features.Recommendations.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

public class RecommendationController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult> GetRecommendations()
    {
        int userId = User.GetUserId();

        var publications = await Mediator.Send(new GetRecommendations.Query { UserId = userId });

        return Ok(publications);
    }
}
