namespace TAABP.Domain.Entities;

public sealed class Amenity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<HotelAmenity> HotelAmenities { get; set; } = new List<HotelAmenity>();
}
