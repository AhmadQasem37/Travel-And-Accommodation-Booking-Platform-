namespace TAABP.Domain.Entities;

public sealed class Room : BaseEntity
{
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int AdultCapacity { get; set; }
    public int ChildCapacity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Description { get; set; }
    public Hotel Hotel { get; set; } = null!;
    public RoomType RoomType { get; set; } = null!;
    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
}
