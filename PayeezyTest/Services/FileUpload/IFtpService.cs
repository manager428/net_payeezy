using PayeezyTest.Models;

namespace PayeezyTest.Services
{
    public interface IFtpService : IDisposable
    {
        Task<bool> UploadFileToFTP();

        void AppendWriteCSV(string referralId, string paidDate, string paidAmount, string transactionId);
    }
}
