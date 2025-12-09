namespace API.Controllers;

using Application.Features.Recommendations.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

public class RecommendationController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult> GetRecommendations()
    {
        int userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetRecommendations.Query { UserId = userId }));
    }
}
