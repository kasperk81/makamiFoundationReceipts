using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Stripe;
using Stripe.Checkout;
using StripeExample;

public class StripeOptions
{
    public string option { get; set; }
}

namespace server.Controllers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost
                .CreateDefaultBuilder(args)
                //Change in production
                .UseUrls("http://0.0.0.0:4242")
                .UseWebRoot("public")
                .UseStartup<Startup>()
                .Build()
                .Run();
            QuestPDF.Settings.License = LicenseType.Community;
            // code in your main method
        }
    }

    public class Startup
    {
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            // This is your test secret API key.
            StripeConfiguration.ApiKey =
                "sk_test_51PY9wlAFhMxzwGjjtnMpqSd7iQcSajbWCsaAq6EIb6cTSKhp3OZ1dGwvEKkBVzKg5UnnH75WmkAFvFlef2jrtDOf00ad9s3hBS";
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseCors(policyBuilder =>
                policyBuilder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("https://makamifoundation.com")
            );
            app.UseRouting();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }

    [Route("transaction")]
    [ApiController]
    public class CheckoutApiController : Controller
    {
        private readonly ILogger<CheckoutApiController> _logger;

        public CheckoutApiController(ILogger<CheckoutApiController> logger)
        {
            _logger = logger;
        }

        [HttpPost("create-checkout-session")]
        public ActionResult Create(DonationRequest request)
        {
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                        Price = "price_1Q0RYQAFhMxzwGjjLuCSBSzB",
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "https://makamifoundation.com/thank-you-for-your-donation/",
                CancelUrl = "https://makamifoundation.com/Donate",
                PaymentIntentData = new()
                {
                    Metadata = new()
                    {
                        { "DonationTowards", request.DonationTowards },
                        { "DonationDetails", request.DonationDetails },
                        { "Phone", request.Phone },
                        { "FullName", request.FullName }
                    }
                },
                CustomerEmail = request.Email,
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(new { redirect_url = session.Url });
        }

        [HttpPost("payment-intent-succeeded")]
        public async Task<ActionResult> SendReceiptEmail()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);

                if (stripeEvent.Type != Events.PaymentIntentSucceeded)
                {
                    // Unexpected event type
                    _logger.LogWarning("Unhandled event type: {0}", stripeEvent.Type);
                    return Ok();
                }
                var intent = stripeEvent.Data.Object as PaymentIntent;

                ChargeService chargeService = new();
                Charge charge = await chargeService.GetAsync(intent.LatestChargeId);
                string emailString = intent.ReceiptEmail;
                //TODO Change to intent.ReceiptNumber in Production
                string receiptNumber = "1111-2222";
                string fullName = intent.Metadata.GetValueOrDefault("FullName");

                if (
                    string.IsNullOrWhiteSpace(emailString)
                    || string.IsNullOrWhiteSpace(receiptNumber)
                )
                {
                    _logger.LogError("Email or receipt number null from Stripe.");
                    return StatusCode(StatusCodes.Status400BadRequest);
                }
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    _logger.LogError("Customer's name not provided");
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                ReceiptData receiptData =
                    new()
                    {
                        Email = emailString,
                        ReceiptNumber = receiptNumber,
                        FullName = fullName,
                        Amount = charge.Amount,
                        DonationDetails = intent.Metadata.GetValueOrDefault("DonationDetails"),
                        InSupportOf = intent.Metadata.GetValueOrDefault("DonationTowards"),
                        Phone = intent.Metadata.GetValueOrDefault("Phone"),
                        Time = DateTimeOffset.UtcNow
                    };

                ReceiptPdfTemplate pdf = new(receiptData);

                MimeMessage email = new MimeMessage();
                MailboxAddress from = new MailboxAddress("Makami Foundation", "donation@mef.com");
                email.From.Add(from);
                email.Subject = "Donation Receipt";
                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody =
                    "<p>Thank you for your donation! Please see the attached PDF receipt of your donation.</p>";
                bodyBuilder.Attachments.Add("Donation Receipt.pdf", pdf.GeneratePdf());
                email.Body = bodyBuilder.ToMessageBody();

                SmtpClient client = new SmtpClient();

                await client.ConnectAsync("proxy.makamicollege.com", 25, SecureSocketOptions.None);

                MailboxAddress To = new MailboxAddress(fullName, intent.ReceiptEmail);
                email.To.Add(To);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
                client.Dispose();

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e.ToString());
                return BadRequest();
            }
            catch (Exception e) when (e is not StripeException)
            {
                _logger.LogError(e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
