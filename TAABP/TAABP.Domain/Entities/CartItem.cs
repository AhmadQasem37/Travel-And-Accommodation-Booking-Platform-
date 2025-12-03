namespace TAABP.Domain.Entities;

public sealed class CartItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal PriceAtBooking { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public Cart Cart { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
