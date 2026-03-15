using Application.Repositories.SpamRepository;
using Application.Repositories.UserActivityLogRepository;
using Application.Services.UserActionLogger;
using Domain.DTOs;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Features.Comments.Commands;

using Application.Core;
using AutoMapper;
using Domain.DTOs.CommentDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class CreateComment
{
    private const int CommentRestrictionInSeconds = 10;
    public class Command : IRequest<Result<CommentDto>>
    {
        public required int UserId { get; set; }
        public required CreateCommentDto Comment { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper,
    ISpamRepository spamRepository, IUserActionLogger<CreateComment> logger)
        : IRequestHandler<Command, Result<CommentDto>>
    {
        private const int MAX_DEPTH = 10;

        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users
                .Include(a => a.ProfileImage)
                .FirstOrDefaultAsync(a => a.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<CommentDto>.Failure("User was not found", 400);

            Publication? publication = await context.Publications
                .FindAsync([request.Comment.PublicationId], cancellationToken);

            if (publication == null)
                return Result<CommentDto>.Failure("Publication you are trying to delete no longer exists", 400);

            bool isSpam = await CheckForCommentSpam(user.Id);
            if (isSpam)
            {
                return Result<CommentDto>.Failure("You are sending comments too fast", 400);
            }
            
            var res = await spamRepository.MakeComment(request.UserId);
            if (res == "Forbidden")
            {
                return Result<CommentDto>.Failure(
                    "You cannot make comments for today due to our antispam rules", 400);
            }
            
            Comment? parentComment = null;
            int? effectiveParentId = request.Comment.ParentCommentId;

            if (effectiveParentId != null)
            {
                parentComment = await context.Comments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == effectiveParentId, cancellationToken);

                if (parentComment == null)
                    return Result<CommentDto>.Failure("Parent comment you are responding to no longer exists", 400);

                if (parentComment.PublicationId != request.Comment.PublicationId)
                    return Result<CommentDto>.Failure("Comment reply internal server error. Try again later!", 500);

                var parentDepth = await context.CommentTrees
                    .Where(x => x.DescendantId == parentComment.Id)
                    .MaxAsync(x => x.Depth, cancellationToken);

                if (parentDepth + 1 > MAX_DEPTH)
                {
                    var targetParentDepth = MAX_DEPTH - 1;
                    var distanceUp = parentDepth - targetParentDepth;

                    var newParentId = await context.CommentTrees
                        .Where(x => x.DescendantId == parentComment.Id && x.Depth == distanceUp)
                        .Select(x => x.AncestorId)
                        .SingleAsync(cancellationToken);

                    effectiveParentId = newParentId;

                    parentComment = await context.Comments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == effectiveParentId, cancellationToken);
                }
            }

            var comment = new Comment
            {
                Author = user,
                Publication = publication,
                Content = request.Comment.Content,
                CreationDate = DateTime.UtcNow,
                ParentCommentId = effectiveParentId
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync(cancellationToken);

            await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""CommentTrees"" (""AncestorId"", ""DescendantId"", ""Depth"")
            VALUES ({comment.Id}, {comment.Id}, 0);
        ", cancellationToken);

            if (effectiveParentId != null)
            {
                await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""CommentTrees"" (""AncestorId"", ""DescendantId"", ""Depth"")
                SELECT ""AncestorId"", {comment.Id}, ""Depth"" + 1
                FROM ""CommentTrees""
                WHERE ""DescendantId"" = {effectiveParentId};
            ", cancellationToken);
            }

            await logger.LogAsync(request.UserId, UserLogAction.CreateComment, new
            {
                info = $"Comment {comment.Id} was created" +
                       $" by user {request.UserId}"
            }, cancellationToken);
            

            return Result<CommentDto>.Success(mapper.Map<CommentDto>(comment));
        }

        // true - it is spam; false - it is not spam
        private async Task<bool> CheckForCommentSpam(int userId)
        {
            var lastComment = await context.Comments.Where(a =>
                    a.AuthorId == userId)
                .OrderByDescending(x => x.CreationDate)
                .FirstOrDefaultAsync();

            if (lastComment == null)
                return false;
            

            var cutOff = DateTime.UtcNow.AddSeconds(-CommentRestrictionInSeconds);
            if (lastComment.CreationDate > cutOff)
            {
                return true;
            }

            return false;
        }
    }

}
