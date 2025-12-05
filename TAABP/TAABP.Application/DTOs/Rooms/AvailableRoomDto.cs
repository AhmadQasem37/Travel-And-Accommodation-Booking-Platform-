namespace TAABP.Application.DTOs.Rooms;

public sealed record AvailableRoomDto(
    Guid RoomId,
    string RoomNumber,
    Guid RoomTypeId,
    string RoomTypeName,
    string? RoomTypeDescription,
    decimal PricePerNight,
    decimal? DiscountedPrice,
    int AdultCapacity,
    int ChildCapacity,
    IReadOnlyList<RoomImageDto> Images);
