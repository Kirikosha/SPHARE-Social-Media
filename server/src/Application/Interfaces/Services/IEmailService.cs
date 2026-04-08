namespace Application.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailsAsync(List<string> emails, string subject, string body, bool isBodyHtml = false);
    Task SendEmailAsync(string email, string subject, string body, bool isBodyHtml = false);
}
