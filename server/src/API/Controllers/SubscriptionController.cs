using Application.Features.Subscription.Commands;
using Application.Features.Subscription.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class SubscriptionController : BaseApiController
{
    [Authorize]
    [HttpPost("subscribe")]
    public async Task<ActionResult> Subscribe([FromBody] string uniqueNameIdentifier)
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator.Send(new Subscribe.Command
        {
            FollowUserUniqueNameIdentifier = uniqueNameIdentifier,
            UserId = userId
        }));
    }

    [Authorize]
    [HttpPost("unsubscribe")]
    public async Task<ActionResult> Unsubscribe([FromBody] string uniqueNameIdentifier)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        return HandleResult(await Mediator.Send(new Unsubscribe.Command
        {
            UnfollowUserUniqueNameIdentifier = uniqueNameIdentifier,
            UserId = userId
        }));
    }

    [HttpGet("subscriptions-count")]
    public async Task<ActionResult> GetSubscriptionsCount([FromQuery] string uniqueNameIdentifier)
    {
        return HandleResult(await Mediator.Send(new GetSubscriptionsCount.Query
        {
            UniqueNameIdentifier = uniqueNameIdentifier
        }));
    }

    [HttpGet("followers-count")]
    public async Task<ActionResult> GetFollowersCount([FromQuery] string uniqueNameIdentifier)
    {
        return HandleResult(await Mediator.Send(new GetFollowersCount.Query
        {
            UniqueNameIdentifier = uniqueNameIdentifier
        }));
    }

    [Authorize]
    [HttpGet("subscriptions")]
    public async Task<ActionResult> GetSubscriptions([FromQuery] string uniqueNameIdentifier)
    {
        return HandleResult(await Mediator.Send(new GetSubscriptions.Query
        {
            UniqueNameIdentifier = uniqueNameIdentifier
        }));
    }

    [Authorize]
    [HttpGet("followers")]
    public async Task<ActionResult> GetFollowers([FromQuery] string uniqueNameIdentifier)
    {
        return HandleResult(await Mediator.Send(new GetFollowers.Query
        {
            UniqueNameIdentifier = uniqueNameIdentifier
        }));
    }

    [Authorize]
    [HttpGet("is-following")]
    public async Task<ActionResult> IsFollowing([FromQuery] string uniqueNameIdentifier)
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator.Send(new IsFollowing.Query
        {
            UniqueNameIdentifier = uniqueNameIdentifier,
            UserId = userId
        }));
    }


}
