namespace Application.Interfaces.Services;

public interface IPasswordService
{
    bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
    (byte[] Hash, byte[] Salt) HashPassword(string password);
}