using System;

namespace StripeExample
{
    public class ReceiptData
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string InSupportOf { get; set; }
        public string DonationDetails { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTimeOffset Time { get; set; }
        public double Amount { get; set; }
    }
}
