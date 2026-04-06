using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;
using System.Net.Mail;
using Application.Interfaces.Services;
using Infrastructure.Settings;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    //Multiple email sending

    public async Task SendEmailsAsync(List<string> emails, string subject, string body, bool isBodyHtml = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));

            foreach (var email in emails)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                _smtpSettings.Server,
                _smtpSettings.Port,
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new SmtpException($"During sending emails an error happened: {ex.Message}");
        }
    }

    // Singular email sending
    public async Task SendEmailAsync(string email, string subject, string body, bool isBodyHtml = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                _smtpSettings.Server,
                _smtpSettings.Port,
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new SmtpException($"During sending email an error happened: {ex.Message}");
        }
    }
}
