namespace Application.Repositories.SpamRepository;

public interface ISpamRepository
{
    Task<bool> MakePublication(string userId, CancellationToken ct = default);
    Task<bool> MakeComment(string userId, CancellationToken ct = default);
    Task<bool> MakeLike(string userId, CancellationToken ct = default);
    Task<bool> MakeComplaint(string userId, CancellationToken ct = default);
}