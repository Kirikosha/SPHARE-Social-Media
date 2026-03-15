using Application.Repositories.UserActivityLogRepository;
using Domain.DTOs;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Features.Comments.Commands;

using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<Result<bool>>
    {
        public required int CommentId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IUserActivityLogRepository logRepository, 
        ILogger<DeleteComment> logger) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            Comment? comment = await context.Comments.Include(a => a.Replies).FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);
            if (comment == null) return Result<bool>.Failure("Comment was not found", 404);

            comment.IsDeleted = true;

            context.Update(comment);

            var res = await logRepository.LogUserAction(new UserActionLogDto()
            {
                UserId = comment.AuthorId,
                ActionType = UserLogAction.DeleteComment,
                AdditionalDescription = JsonConvert.SerializeObject(new { info = "Deletion was successful" }),
                ExecutedAt = DateTime.UtcNow
            });

            if (!res)
            {
                logger.Log(LogLevel.Error, "User action was not logged. User id: " + comment.AuthorId);
            }
            
            return Result<bool>.Success(true);
        }
    }
}
