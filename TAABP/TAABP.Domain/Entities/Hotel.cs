namespace TAABP.Domain.Entities;

public sealed class Hotel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StarRating { get; set; }
    public Guid CityId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal MinRoomPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public string? NearbyAttractions { get; set; }
    public City City { get; set; } = null!;
    public User? Owner { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<HotelImage> Images { get; set; } = new List<HotelImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<HotelAmenity> HotelAmenities { get; set; } = new List<HotelAmenity>();
    public ICollection<RecentlyVisitedHotel> RecentlyVisitedHotels { get; set; } = new List<RecentlyVisitedHotel>();
}
