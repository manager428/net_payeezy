using PayeezyTest.Models;
using System.ComponentModel.DataAnnotations;

namespace PayeezyTest.Dto
{
    public class PaymentDetailDto
    {
        [Required(ErrorMessage = "CardHolderName is required.")]
        public string CardHolderName{ get; set; }

        [Required(ErrorMessage = "CardNumber is required.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "ExpireDate is required.")]
        public string ExpireDate { get; set; }

        [Required(ErrorMessage = "Cvv is required.")]
        public string Cvv { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public double Amount { get; set; }

        public int ReferalId { get; set; }

        public PatientCoPay PatientCoPay { get; set; }
    }
}
