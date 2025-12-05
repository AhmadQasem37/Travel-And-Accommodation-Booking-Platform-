using FluentValidation;

namespace TAABP.Application.Features.Hotels.Queries.GetRecentlyVisitedHotels;

public sealed class GetRecentlyVisitedHotelsQueryValidator : AbstractValidator<GetRecentlyVisitedHotelsQuery>
{
    public GetRecentlyVisitedHotelsQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20)
            .WithMessage("Count must be between 1 and 20");
    }
}
