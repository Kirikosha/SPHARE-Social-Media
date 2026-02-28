namespace Application.Repositories.SpamRepository;

public interface ISpamRepository
{
    Task<string> MakePublication(int userId);
    Task<string> MakeComment(int userId);
    Task<string> MakeLike(int userId);
    Task<string> MakeComplaint(int userId);
}