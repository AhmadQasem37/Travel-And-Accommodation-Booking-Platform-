namespace TAABP.Domain.Entities;

public sealed class City : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? PostOffice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}
