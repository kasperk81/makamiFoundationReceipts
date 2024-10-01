using System.ComponentModel.DataAnnotations;

namespace StripeExample
{
    public class DonationRequest
    {
        [Required]
        public string Email { get; set; }
        public string DonationTowards { get; set; }
        public string DonationDetails { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
    }
}
