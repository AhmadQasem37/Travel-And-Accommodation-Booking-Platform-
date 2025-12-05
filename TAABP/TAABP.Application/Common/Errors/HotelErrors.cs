namespace TAABP.Application.Common.Errors;

public static class HotelErrors
{
    public static Error NotFound(Guid id) =>
        new("Hotel.NotFound", $"Hotel with ID '{id}' was not found.");
}
