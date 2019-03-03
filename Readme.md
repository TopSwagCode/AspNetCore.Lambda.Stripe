# TopSwagCode.Lambda.Payment

There are 2 ways we can tackle the task of creating a AWS Lambda function and deploy it. 

* The uber 1337 h4x0r way (.NET Core CLI)
* Quick and dirty (AWS toolkit for Visual studio)

Jokeing aside I will be focusing on showing how to get a quick prototype up and running using the AWS toolkit for Visual studio.

# Requirements
* To get started you should of course have Visual Studio 2017 installed. (Older version's supported, but I have not tried them out.)
* Have .NET Core 2.1 installed on your machine. The version currently supported by AWS.
* Get the AWS Toolkit https://aws.amazon.com/visualstudio/

# Getting started

When you have installed the AWS Toolkit, you should see a new project template under C# tab in Visual Studio as seen below here: 

![Create Lambda function](docs/CreateProject.png "Create Lambda function")

For this sample, I will keep it simple and create a default AWS Lambda Project without tests. Without going to much into details the AWS Serverlesss Application is a way of creating an normal WebAPI project and hosting it as a Lambda Function. ( Really awesome. Take a look at it when you got a grip on AWS Lambda)

When you picked the project type you get a list of blueprint's to pick from. We will be starting from scratch by picking "Empty Function". But try giving the other blueprint's a quick look. You might find something awesome for your next project.

![Project type](docs/ProjectType.png "Project type")

Now Take a quick look at the code

![Project start](docs/Start.png "Start")

A simple project that takes some input and makes it all uppercase. Let try it out and see it working use the Mock Lambda Test Tool.

![Debug](docs/Debug.png "Debug")

Your default browser should now open and look like the following:

![Mock Lambda](docs/MockLambda.png "Mock Lambda")

Try play around and perhaps add a breakpoint so you can see you can debug it. Below here you can see my test call working.

![Mock Lambda](docs/DebugResult.png "Mock Lambda")

Well that's all fine, but not very serverless running some code on your own local machine. Let's deploy it to AWS. Right click your project and pick Publish to AWS Lambda.

![Lambda publish](docs/PublishStart.png "Lambda publish")

You should see a publish wizard that help's you deploy the Lambda Function. Simply give your function a name. I will just be naming my function StripeServerless. Press next.

![Publish wizard](docs/PublishWizard.png "Publish wizard")

The next step before deploying your Lambda function is to pick the Role in which your function will be running. It is important that you pick a role that matches what accesses you will be needing. (Not just allow all for all your code)

I will be picking the Default Lambda role

![Publish wizard](docs/PublishWizard2.png "Publish wizard")

If you don't have a role already created they have a long list of default roles to pick from.

![Roles](docs/Roles.png "Roles")

Now just press upload and wait for it to be done.

You should now see a new tab. This is a small test tab to see your Lambda function in action deployed. 

![Live test](docs/LiveTest.png "Live test")

As before try your Lambda function out and see it is still working.

![Live test result](docs/LiveTestResult.png "Live test result")

But Holy cow batman?! I was just billed 1400ms for something so simply. You can see it in the log output.

![Cold start](docs/ColdStart.png "cold start")

If you try to run it a few more time you see the duration greatly decreasing. The reason is that 
AWS lambda has Cold starts which happens when it hasn't been used for a while. It is not a focus for this small guide. If you want to read more google it or look [Here.]("https://hackernoon.com/cold-starts-in-aws-lambda-f9e3432adbf0") 


# API

So we have a running AWS Lambda funciton. The only problem is it is not wery usefull. We don't have any easy way of running it. So the next step should be making it accessible from the web. You will need to logon AWS console and goto your deployed Lambda funciton.

![Lambda web](docs/LambdaWeb.png "Lambda web")

AWS Lambda uses triggers to interact with your Lambda Function. They have a range of cool triggers, but we will be focusing on getting our Lambda function to be accessed from the web by using the API Gateway.

Pick the API Gateway and configure it. 
I will be picking "Create a new API" and for Security I will pick "Open" so it can be used for my Stripe Payments. (Remember to press save to save your new trigger.)

![Api Gateway](docs/CreateAPI.png "Api Gateway")

When you have created the API Gateway you will find your link:

![Api Gateway](docs/APICreated.png "Api Gateway")

If you click it you will get response

```
{"message": "Internal server error"}
```

This is because the Lambda function we just created does not support to be invoked by API Gateway.

We now need to go back to the code and make some changes to support API Gateway. First of install a new Nuget package: "Amazon.Lambda.APIGatewayEvents".

![Nuget](docs/Nuget.png "Nuget")

Now we can go back to our Function.cs and use this new package. There are 2 important changes that needs to be done.

* Change Input to be APIGatewayProxyRequest
* Change Return to be APIGatewayProxyResponse

![Code change](docs/CodeChange.png "Code change")

If we now try to run the code again using the Mock Lambda tool from AWS. This time instead of writing function input your self, pick a sample request from the dropdown. "Pick API Gateway AWS Proxy". This will create a big sample request with request body {"test": "body"}. If you try to execute this function you should see the request body returned as uppercase.

![Debug Api](docs/DebugAPI.png "Debug Api")

The lambda function now supports API Gateway and we can deploy it again as before. Right click the project and publish your AWS Function. It should remember your setings from last time and you should simply press upload. As last time a new tab will open with your deployed Lambda function. Pick a sample request as we did for Mock service using the API Gateway AWS Proxy. Now we can see the Lambda function running live.

![Live test](docs/LiveTest2.png "Live test")

If we want to see it in real action. Open your API test tool of choice. I will be using Postman for this. Find the URL for the API Gateway on your Lambda function and try calling it with some JSON.

![Api test](docs/Postman.png "Api test")

You now have a running Lambda function that can be called from the web. Being that your website, a mobile app or something else :)

![Who is awesome](docs/Awesome.jpg "Who is awesome")

# Show me the money

Now it is all wired, it's time to make some real custom code and using the Stripe SDK for getting payments.

First lets make a simple html client that will use stripe client to get the payment information.

You can find the quickstart guide for Stripe [here.](https://stripe.com/docs/quickstart)
If you are logged in the samples on their page already contains your test keys for stripe.

My sample client looks like the following after I have changed the form action to point to the AWS Lambda function.

```html
<html>
    <body>
        <form action="https://da62ix0922.execute-api.eu-west-1.amazonaws.com/default/StripeServerless" method="POST">
            <script
              src="https://checkout.stripe.com/checkout.js" class="stripe-button"
              data-key="pk_test_qz5lvl9kE1TnPrA6FgXSHUGk"
              data-amount="999"
              data-name="TopSwagCode"
              data-description="Example charge"
              data-image="https://stripe.com/img/documentation/checkout/marketplace.png"
              data-locale="auto"
              data-currency="dkk">
            </script>
          </form>
    </body>
</html>
```

which will give you a page that looks like the following:

![Simple client](docs/SimpleClient.png "Simple client")

Pressing the button will get you a checkout form like this one:

![Simple client](docs/SimpleClient2.png "Simple client")

This is just a simple payment client. Stripe has guides for IOS, Android and Web. You can do tons of stuff. Just follow their guides. For now I will stick with the simple checkout.

Using the simple client we created before the request we get when a user submits a payment will look like the following:

![Stripe request](docs/StripeRequest.png "Stripe request")

Not prettified:

```
stripeToken=tok_1E9gPVGDSTf7xFZQ78Mh21DS&stripeTokenType=card&stripeEmail=jor%40firstagenda.com
```

This is the input our Lambda function will be receiving in the request.

Now it is time to get back into our Lambda Function code.
The next steps:

* Install StripeSDK nuget package
* Get Form Data
* Create Charge
* Return Charged

So lets move along by getting the Stripe.net nuget package

![Stripe nuget](docs/StripeNuget.png "Stripe nuget")

I have put all my code into Function.cs and it looks like the following:

```cs
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
            StripeConfiguration.SetApiKey("sk_test_StripeSecretApiKey"); // Change with your own
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

```

Changes made to the code:

* Added constructor with setup of Stripe.net 
* Added StripePayment Request and Response models
* Added GetStripePaymentRequestFromFormDataString
* Added Create Customer and Create Charge using Stripe.net
* Added combined response from customer and charge result and returning it

Now all that is left is deploying the code and test it all works. So right click your project and upload your lambda function again. 

Now let's try to open the TestClient and make a payment. It should take a couple of seconds to complete the payment because of cold start. If you run you Lambda function a couple of times you should see that it runs quick. The response from making a payment should be something like this:

```
{"Amount":999,"Paid":true,"Email":"jor@firstagenda.com"}
```

Now let's check in Stripe dashboard that we can see the payment there aswell.

![Stripe payment](docs/StripePayment.png "Stripe payment")

If your not seeing your payment remember to switch Viewing test data on

![Test data](docs/TestData.png "Test data")

That's it. You now have a way of receiving serverless payments

# Next steps:

![Automate](docs/Automate.jpg "Automate")

All we have done can also be achieved in the command line. This is really awesome if you want to setup a CI/CD for your lambda function. You can read more about the [Lambda .NET Core CLI here.](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-coreclr-deployment-package.html)

