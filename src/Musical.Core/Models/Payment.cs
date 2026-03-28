namespace Musical.Core.Models;

public class Payment
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public required string Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
