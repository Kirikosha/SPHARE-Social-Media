namespace Application.Services.PasswordResetService;

using Application.Features.Users.Queries;
using Application.Services.EmailService;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

public class PasswordResetService : IPasswordResetService
{
    private readonly IMemoryCache cache;
    private readonly IMediator mediator;
    private readonly IEmailService emailService;
    private readonly int _codeExpiryMinutes = 10;

    public PasswordResetService(IMediator mediator, IEmailService emailService, IMemoryCache cache)
    {
        this.cache = cache;
        this.mediator = mediator;
        this.emailService = emailService;
    }

    public async Task<string> GenerateAndStoreResetCode(string email)
    {

        var user = await mediator.Send(new GetRawUserByEmail.Query { Email = email });
        if (user == null) return string.Empty;

        var code = GenerateRandomCode(10);

        cache.Set(email, code, TimeSpan.FromMinutes(_codeExpiryMinutes));

        await emailService.SendEmailAsync(
            email,
            "Password Reset Code",
            $"Your password reset code is: {code}\nThis code will expire in {_codeExpiryMinutes} minutes.",
            false
        );

        return code;
    }

    public Task<bool> VerifyResetCode(string email, string code)
    {
        if (cache.TryGetValue(email, out string? storedCode) && storedCode == code)
        {
            cache.Remove(email);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
