using FluentValidation;

namespace TAABP.Application.Features.Hotels.Queries.GetFeaturedDeals;

public sealed class GetFeaturedDealsQueryValidator : AbstractValidator<GetFeaturedDealsQuery>
{
    public GetFeaturedDealsQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20)
            .WithMessage("Count must be between 1 and 20");
    }
}
