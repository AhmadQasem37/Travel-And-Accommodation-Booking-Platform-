namespace TAABP.Application.DTOs.Reviews;

public sealed record ReviewDto(
    Guid Id,
    Guid UserId,
    string UserName,
    int Rating,
    string Content,
    DateTime CreatedAt);
