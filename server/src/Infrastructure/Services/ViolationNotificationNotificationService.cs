using Application.Core;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Infrastructure.Services;

public class ViolationNotificationNotificationService(IEmailService emailService, IUserService userService, 
    IViolationService violationService) : IViolationNotificationService
{
    public async Task<Result<bool>> RegisterViolationAsync(Violation violation, int scoreIncrease, bool 
            isPublication, CancellationToken ct)
    {
        string item = isPublication ? "publication" : "comment";
        string body = MakeBody(isPublication, violation.ViolationText, item);
        try
        {
            var violationCreationResult = await violationService.CreateViolation(violation, ct);

            var violationScoreUpdateResult = await userService.UpdateViolationScore(violation.ViolatedById,
                scoreIncrease, ct);

            if (!violationCreationResult.Value || !violationScoreUpdateResult.Value) return false;
            var userEmail = await userService.GetUserEmailByIdAsync(violation.ViolatedById, ct);
            if (string.IsNullOrEmpty(userEmail))
                return false;
            await emailService.SendEmailAsync(
                userEmail,
                $"Your {item} was removed by administration",
                body,
                true);
            return true;
        }
        catch
        {
            return false;
        }
    }
    private string MakeBody(bool isPublication, string removalReason, string item)
    {

        string message = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ padding: 20px; border: 1px solid #ddd; border-radius: 8px; }}
        .reason {{ background-color: #f9f9f9; padding: 10px; border-left: 4px solid #e74c3c; margin-top: 10px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h2>Notice of Content Removal</h2>
        <p>Dear user,</p>
        <p>Your recent <strong> {item}</strong> has been removed due to a violation of our community guidelines.</p>
        <p><strong>Reason:</strong></p>
        <div class=""reason"">
            {removalReason}
        </div>
        <p>Your violation score has been increased. Continued violations may result in account restrictions.</p>
        <p>Thank you for your understanding.</p>
        <p>The Moderation Team</p>
    </div>
</body>
</html>
            ";
        return message;
    }
}
