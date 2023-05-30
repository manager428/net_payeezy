using PayeezyTest.Models;

namespace PayeezyTest.Services
{
    public class PatientService : IPatientService
    {
        private readonly PayeezyDbContext _context;

        public PatientService(PayeezyDbContext context)
        {
            _context = context;

        }

        public async Task<PatientCoPay?> GetPatientCoPayData(int referalID)
        {
            
            var patient = _context.PatientCoPay.Where(x => x.Appointment_ReferralID == referalID).FirstOrDefault();
            return await Task.FromResult(patient);
        }
        
        public async Task UpdatePatientCoPaymentPaid(int referalID, string status, string transactionID, double amount)
        {
            var patient = _context.PatientCoPay.Where(x => x.Appointment_ReferralID == referalID).FirstOrDefault();

            if (patient != null)
            {
                patient.Status = status;
                patient.TransactionID = transactionID;
                patient.PaymentAmount = amount;
                patient.PaymentDate = DateTime.Now;
                _context.PatientCoPay.Update(patient);
                await _context.SaveChangesAsync();
            }

        }
        public async Task<bool> CreatePatientCoPay(PatientCoPay patientCoPay)
        {
            var patient = _context.PatientCoPay.Where(x => x.Appointment_ReferralID == patientCoPay.Appointment_ReferralID).FirstOrDefault();

            if (patient == null)
            {
                await _context.PatientCoPay.AddAsync(patientCoPay);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public void Dispose()
        {

        }
    }
}
