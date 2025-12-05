using TAABP.Application.Common;

namespace TAABP.Application.Common.Errors;

public static class RoomErrors
{
    public static Error NotFound(Guid id) =>
        new("Room.NotFound", $"Room with ID '{id}' was not found.");

    public static Error NotAvailable(Guid id) =>
        new("Room.NotAvailable", $"Room with ID '{id}' is not available.");

    public static Error AlreadyBooked(Guid id, DateOnly checkIn, DateOnly checkOut) =>
        new("Room.AlreadyBooked", $"Room with ID '{id}' is already booked for the dates {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}.");
}
