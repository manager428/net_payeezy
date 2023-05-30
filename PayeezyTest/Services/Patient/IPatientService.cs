using PayeezyTest.Models;

namespace PayeezyTest.Services
{
    public interface IPatientService : IDisposable
    {
        Task<PatientCoPay?> GetPatientCoPayData(int referalID);
        Task UpdatePatientCoPaymentPaid(int referalID, string status, string transactionID, double amount);

        Task<bool> CreatePatientCoPay(PatientCoPay patientCoPay);
    }
}
