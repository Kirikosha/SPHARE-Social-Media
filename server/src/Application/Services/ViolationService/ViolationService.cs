using Application.Features.Users.Commands;
using Application.Features.Violations.Commands;
using Application.Services.EmailService;

namespace Application.Services.ViolationService;

public class ViolationService(IEmailService emailService, IMediator mediator) : IViolationService
{
    public async Task<bool> RegisterViolationAsync(User user, Violation violation, int scoreIncrease, bool isPublication)
    {
        string item = isPublication ? "publication" : "comment";
        string body = MakeBody(isPublication, violation.ViolationText, item);
        try
        {
            var violationCreationResult = await mediator.Send(new CreateViolation.Command { Violation = violation });

            var violationScoreUpdateResult = await mediator.Send(new UpdateViolationScore.Command
            {
                ScoreIncreaseValue = scoreIncrease,
                UserId = user.Id
            });

            if (!violationCreationResult.Value || !violationScoreUpdateResult.Value) return false;
            await emailService.SendEmailAsync(
                user.Email,
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
