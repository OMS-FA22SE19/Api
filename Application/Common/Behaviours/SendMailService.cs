using Application.Users.Commands;
using System.Net.Mail;
using System.Net;
using Application.Common.Interfaces;

namespace Application.Common.Behaviours
{
    public class SendMailService : ISendMailService
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.To.Add(email);
            mailMessage.Subject = subject;
            mailMessage.From = new MailAddress("omsfa22se19@gmail.com");
            mailMessage.Body = htmlMessage;
            mailMessage.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Host = "smtp.gmail.com";
            smtp.Credentials = new NetworkCredential("omsfa22se19@gmail.com", "lffgtvagdhhodlek");
            smtp.Send(mailMessage);
        }

        public async Task SendMail(MailContent mailContent)
        {
            await SendEmailAsync(mailContent.To, mailContent.Subject, mailContent.Body);
        }
    }
}
