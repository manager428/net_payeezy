using System.ComponentModel.DataAnnotations;

namespace PayeezyTest.Models
{
    public class PatientCoPay
    {
        [Key]
        public int Appointment_ReferralID { get; set; }
        public int PatientID { get; set; }
        public string PatientLN { get; set; }
        public string PatientFN { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }
        public string InsuranceCoName { get; set; }
        public double AmountDue { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime ADate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double? PaymentAmount { get; set; }
        public string? Status { get; set; }
        public string? TransactionID { get; set; }
    }
}
