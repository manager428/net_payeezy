using PayeezyTest.Models;

namespace PayeezyTest.Services
{
    public interface IEmailSender
    {
        /// <summary>
        /// Send Email Async
        /// </summary>
        /// <param name="email">An Email</param>
        /// <param name="subject">The Subject</param>
        /// <param name="message">The Message</param>
        /// <returns>Empty</returns>
        Task SendEmailAsync(string email, string subject, string message);
    }
}
