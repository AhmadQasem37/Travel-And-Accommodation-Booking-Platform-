namespace TAABP.Domain.Entities;

public sealed class BookingItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
    public Booking Booking { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
