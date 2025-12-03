using TAABP.Domain.Enums;

namespace TAABP.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public Guid UserId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? SpecialRequests { get; set; }
    public User User { get; set; } = null!;
    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
}
