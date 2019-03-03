using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Stripe;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StripeServerless
{
    public class Function
    {
        private readonly CustomerService _customerService;
        private readonly ChargeService _chargeService;

        public Function()
        {
            StripeConfiguration.SetApiKey("sk_test_StripeSecretApiKey");
            _customerService = new CustomerService();
            _chargeService = new ChargeService();
        }

        /// <summary>
        /// A simple function that makes a stripe payment.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var stripePaymentRequest = GetStripePaymentRequestFromFormDataString(request.Body);

            var customer = _customerService.Create(new CustomerCreateOptions
            {
                Email = stripePaymentRequest.stripeEmail,
                SourceToken = stripePaymentRequest.stripeToken
            });

            var charge = _chargeService.Create(new ChargeCreateOptions
            {
                Amount = 999,
                Description = "Awesome transaction",
                Currency = "dkk",
                CustomerId = customer.Id
            });

            var response = new StripePaymentResponse
            {
                Amount = charge.Amount,
                Paid = charge.Paid,
                Email = customer.Email
            };

            return new APIGatewayProxyResponse()
            {
                Body = JsonConvert.SerializeObject(response),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// A simple method that takes a FormData request body translates it into a StripePaymentRequest.
        /// The request should look something like the following:
        /// stripeToken=tok_1E9gPVGDSTf7xFZQ78Mh21DS&stripeTokenType=card&stripeEmail=jor%40firstagenda.com
        /// </summary>
        /// <param name="query"></param>
        /// <param name="context"></param>
        /// <returns>The method returns a StripePaymentRequest</returns>
        public StripePaymentRequest GetStripePaymentRequestFromFormDataString(string query)
        {
            var dict = HttpUtility.ParseQueryString(query);
            string json = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
            StripePaymentRequest respObj = JsonConvert.DeserializeObject<StripePaymentRequest>(json);

            return respObj;
        }
    }

    public class StripePaymentRequest
    {
        public string stripeToken { get; set; }
        public string stripeEmail { get; set; }
    }

    public class StripePaymentResponse
    {
        public long Amount { get; set; }
        public bool Paid { get; set; }
        public string Email { get; set; }
    }
}
