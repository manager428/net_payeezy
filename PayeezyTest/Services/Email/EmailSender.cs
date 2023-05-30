using Microsoft.Extensions.Options;
using PayeezyTest.Dto;
using PayeezyTest.Models;
using System.Net;
using System.Net.Mail;

namespace PayeezyTest.Services
{
    public class EmailSender : IEmailSender
    {
        #region Constructor

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        #endregion

        /// <summary>
        /// Email Settings
        /// </summary>
        public EmailSettings _emailSettings { get; }

        /// <summary>
        /// Send Email Async
        /// </summary>
        /// <param name="email">An Email</param>
        /// <param name="subject">The Subject</param>
        /// <param name="message">The Message</param>
        /// <returns>Empty</returns>
        public Task SendEmailAsync(string email, string subject, string message)
        {
            Execute(email, subject, message).Wait();
            return Task.FromResult(0);
        }

        /// <summary>
        /// This Method is used for sending mail to recepient
        /// </summary>
        /// <param name="email">An Email</param>
        /// <param name="subject">The Subject</param>
        /// <param name="message">The Message</param>
        /// <returns>Empty</returns>
        public async Task Execute(string email, string subject, string message)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email)
                                 ? _emailSettings.ToEmail
                                 : email;
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.FromEmail, "Nocturna Payments")
                };
                mail.To.Add(new MailAddress(toEmail));
                
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }

            catch (Exception ex)
            {
            }
        }
    }
}
