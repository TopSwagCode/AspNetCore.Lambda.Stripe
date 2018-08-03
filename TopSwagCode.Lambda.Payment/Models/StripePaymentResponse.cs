namespace TopSwagCode.Lambda.Payment.Models
{
    public class StripePaymentResponse
    {
        public string PaymentProviderToken { get; set; }
        public string Email { get; set; }
        public string Amount { get; set; }
        public string BillingAddressCountry { get; set; }
        public string BillingAddressCity { get; set; }
        public string BillingAddressState { get; set; }
        public string BillingAddressZip { get; set; }
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingName { get; set; }
        public string BusinessVatId { get; set; }
    }
}
