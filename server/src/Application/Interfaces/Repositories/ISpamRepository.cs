using Application.Core;

namespace Application.Interfaces.Repositories;

public interface ISpamRepository
{
    Task<bool> MakePublication(string userId, CancellationToken ct);
    Task<bool> MakeComment(string userId, CancellationToken ct);
    Task<bool> MakeLike(string userId, CancellationToken ct);
    Task<bool> MakeComplaint(string userId, CancellationToken ct);
    Task<Result<Unit>> CreateSpamRating(string userId, CancellationToken ct);
    Task<bool> IsPublicationSpamming(string userId, DateOnly creationDate, CancellationToken ct);
}