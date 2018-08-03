using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Stripe;
using TopSwagCode.Lambda.Payment.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TopSwagCode.Lambda.Payment
{
    public class Function
    {
        private readonly StripeCustomerService _customerService;
        private readonly StripeChargeService _stripeChargeService;

        public Function()
        {
            StripeConfiguration.SetApiKey("sk_live_###################");
            _customerService = new StripeCustomerService();
            _stripeChargeService = new StripeChargeService();
        }
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Console.WriteLine($"Request bodt: {request.Body}");
            var stripePaymentRequest = GetFromQueryString(request.Body);

            var createOptions = new StripeCustomerCreateOptions()
            {
                Email = stripePaymentRequest.stripeEmail,
                SourceToken = stripePaymentRequest.stripeToken
            };

            var customer = _customerService.Create(createOptions);

            Console.WriteLine($"Created customer for email: {customer.Email} with id: {customer.Id}");

            var stripeChargeCreateOptions = new StripeChargeCreateOptions()
            {
                Amount = stripePaymentRequest.Amount,
                Description = "Donation at TopSwagCode",
                Currency = "usd",
                CustomerId = customer.Id
                
            };

            var charge = _stripeChargeService.Create(stripeChargeCreateOptions);
            
            Console.WriteLine($"Created charge for customer: {customer.Id} for amount: {charge.Amount}");

            var responseBody = new StripePaymentResponse()
            {
                Amount = stripePaymentRequest.Amount.ToString(),
                Email = stripePaymentRequest.stripeEmail,
            };

            return new APIGatewayProxyResponse()
            {
                Body = JsonConvert.SerializeObject(responseBody),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" }},
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public StripePaymentRequest GetFromQueryString(string query)
        {
            var dict = HttpUtility.ParseQueryString(query);
            string json = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
            StripePaymentRequest respObj = JsonConvert.DeserializeObject<StripePaymentRequest>(json);

            return respObj;
        }
    }
}
