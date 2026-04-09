using Application.Core.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

using Application.Features.Recommendations.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class RecommendationController : BaseApiController
{
    [HttpGet()]
    public async Task<ActionResult> GetRecommendations([FromQuery] PaginationParams paginationParams)
    {
        string userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetRecommendations.Query { Page = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize, UserId = userId }));
    }
}
