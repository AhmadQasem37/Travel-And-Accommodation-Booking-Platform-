using FluentValidation;

namespace TAABP.Application.Features.Cities.Queries.GetTrendingDestinations;

public sealed class GetTrendingDestinationsQueryValidator
    : AbstractValidator<GetTrendingDestinationsQuery>
{
    public GetTrendingDestinationsQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20)
            .WithMessage("Count must be between 1 and 20.");
    }
}
