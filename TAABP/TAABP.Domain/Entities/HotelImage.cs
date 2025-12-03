namespace TAABP.Domain.Entities;

public sealed class HotelImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HotelId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public Hotel Hotel { get; set; } = null!;
}
