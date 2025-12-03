namespace TAABP.Domain.Entities;

public sealed class RecentlyVisitedHotel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid HotelId { get; set; }
    public DateTime VisitedAt { get; set; }
    public User User { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
}
