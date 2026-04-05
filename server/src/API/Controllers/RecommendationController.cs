namespace API.Controllers;

using Application.Features.Recommendations.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

public class RecommendationController : BaseApiController
{
    [HttpGet("{id}")]
    public async Task<ActionResult> GetRecommendations(string id)
    {
        //string userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetRecommendations.Query { Page = 0, PageSize = 2, UserId = id }));
    }
}
