using Application.Features.Complaints.Commands;
using Application.Features.Complaints.Queries;
using Application.Helpers;
using Domain.DTOs.ComplaintDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace API.Controllers;
public class ComplaintController : BaseApiController
{
    [HttpPost("publication")]
    public async Task<ActionResult> RegisterPublicationComplaint(CreateComplaintDto complaint)
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator
            .Send(new CreatePublicationComplaint.Command { Complaint = complaint, UserId = userId }));
    }

    [HttpPost("comment")]
    public async Task<ActionResult> RegisterCommentComplaint(CreateComplaintDto complaint)
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator
            .Send(new CreateCommentComplaint.Command { Complaint = complaint, UserId = userId }));
    }

    [HttpGet("publication")]
    public async Task<ActionResult> GetPublicationComplaints()
    {
        return HandleResult(await Mediator
            .Send(new GetPublicationComplaints.Query()));
    }

    [HttpGet("comment")]
    public async Task<ActionResult> GetCommentComplaints()
    {
        return HandleResult(await Mediator
            .Send(new GetCommentComplaints.Query()));
    }

    [HttpGet("publication-grouped")]
    public async Task<ActionResult> GetPublicationComplaintsGrouped()
    {
        return HandleResult(await Mediator
            .Send(new GetGroupedComplaints.Query()));;;;
    }
}
