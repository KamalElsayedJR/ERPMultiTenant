using FluentValidation;

namespace ERPMultiTenant.Application.Features.Departments.List;

public sealed class GetDepartmentsQueryValidator : AbstractValidator<GetDepartmentsQuery>
{
    public GetDepartmentsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("Page size must be between 1 and 200.")
            .When(x => x.PageSize.HasValue);
    }
}
