namespace TopSwagCode.Lambda.Payment.Models
{
    public class StripePaymentRequest
    {
        public string stripeToken { get; set; }
        public string stripeEmail { get; set; }
        public int Amount { get; set; }
    }
}
