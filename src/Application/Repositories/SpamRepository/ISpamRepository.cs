namespace Application.Repositories.SpamRepository;

public interface ISpamRepository
{
    Task<string> MakePublication(string userId);
    Task<string> MakeComment(string userId);
    Task<string> MakeLike(string userId);
    Task<string> MakeComplaint(string userId);
}