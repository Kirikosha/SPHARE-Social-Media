namespace Application.Services.EmailService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailsAsync(List<string> emails, string subject, string body, bool isBodyHtml = false);
    Task SendEmailAsync(string email, string subject, string body, bool isBodyHtml = false);
}
