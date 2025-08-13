namespace Application.Services.PasswordResetService;
public interface IPasswordResetService
{
    Task<string> GenerateAndStoreResetCode(string email);
    Task<bool> VerifyResetCode(string email, string code);
}
