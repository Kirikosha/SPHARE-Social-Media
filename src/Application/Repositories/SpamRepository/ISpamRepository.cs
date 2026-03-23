namespace Application.Repositories.SpamRepository;

public interface ISpamRepository
{
    Task<bool> MakePublication(string userId);
    Task<bool> MakeComment(string userId);
    Task<bool> MakeLike(string userId);
    Task<bool> MakeComplaint(string userId);
}