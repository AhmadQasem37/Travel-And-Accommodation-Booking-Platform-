using TAABP.Domain.Enums;

namespace TAABP.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<RecentlyVisitedHotel> RecentlyVisitedHotels { get; set; } = new List<RecentlyVisitedHotel>();
    public ICollection<Hotel> OwnedHotels { get; set; } = new List<Hotel>();
    public Cart? Cart { get; set; }
}
