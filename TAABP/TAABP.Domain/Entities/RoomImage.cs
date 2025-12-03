namespace TAABP.Domain.Entities;

public sealed class RoomImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public Room Room { get; set; } = null!;
}
