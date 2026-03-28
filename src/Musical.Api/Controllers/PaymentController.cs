using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Core.Models;
using Stripe;
using Stripe.Checkout;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(MusicalDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("create-checkout-session")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
    {
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        var priceAmount = config.GetValue("Stripe:PriceAmountCents", 3000L);
        var name = config["Stripe:ProductName"] ?? "Study Sax - Full Access";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = priceAmount,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = name,
                            Description = "One-time purchase for full access to Study Sax"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = request.SuccessUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = request.CancelUrl
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        var payment = new Payment
        {
            UserId = "pending",
            StripeSessionId = session.Id,
            Status = "Pending",
            Amount = (decimal)priceAmount / 100m,
            Currency = "usd"
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        return Ok(new { sessionId = session.Id, url = session.Url });
    }

    [HttpGet("verify-session/{sessionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifySession(string sessionId)
    {
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        var service = new SessionService();

        Session session;
        try
        {
            session = await service.GetAsync(sessionId);
        }
        catch
        {
            return BadRequest(new { message = "Invalid session." });
        }

        if (session.PaymentStatus != "paid")
            return BadRequest(new { message = "Payment not completed.", paid = false });

        var payment = await db.Payments.FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);
        if (payment is null)
            return NotFound(new { message = "Payment record not found." });

        if (payment.UserId != "pending")
            return BadRequest(new { message = "This payment has already been used to create an account." });

        return Ok(new
        {
            paid = true,
            email = session.CustomerDetails?.Email ?? "",
            sessionId
        });
    }

    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(new
        {
            hasPurchased = await db.Payments.AnyAsync(p => p.UserId == userId && p.Status == "Completed")
        });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = config["Stripe:WebhookSecret"];

        Event stripeEvent;
        if (!string.IsNullOrEmpty(webhookSecret))
        {
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
        }
        else
        {
            stripeEvent = EventUtility.ParseEvent(json);
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            if (session is not null)
            {
                var payment = await db.Payments.FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
                if (payment is not null)
                {
                    payment.StripePaymentIntentId = session.PaymentIntentId;
                    if (payment.UserId != "pending")
                    {
                        payment.Status = "Completed";
                        payment.CompletedAt = DateTime.UtcNow;
                    }
                    await db.SaveChangesAsync();
                }
            }
        }

        return Ok();
    }
}

public record CreateCheckoutRequest(string SuccessUrl, string CancelUrl);
