namespace TAABP.Domain.Entities;

public sealed class RoomType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
