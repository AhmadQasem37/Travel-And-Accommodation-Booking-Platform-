namespace TAABP.Domain.Entities;

public sealed class Review : BaseEntity
{
    public Guid HotelId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public Hotel Hotel { get; set; } = null!;
    public User User { get; set; } = null!;
}
