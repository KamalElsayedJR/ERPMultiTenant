using FluentValidation;

namespace ERPMultiTenant.Application.Features.Employees.List;

public sealed class GetEmployeesQueryValidator : AbstractValidator<GetEmployeesQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "employeenumber",
        "name",
        "hiredate",
        "salary",
        "department"
    ];

    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetEmployeesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("Page size must be between 1 and 200.")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum salary must be non-negative.")
            .When(x => x.MinSalary.HasValue);

        RuleFor(x => x.MaxSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum salary must be non-negative.")
            .When(x => x.MaxSalary.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinSalary.HasValue || !x.MaxSalary.HasValue || x.MaxSalary >= x.MinSalary)
            .WithMessage("Maximum salary must be greater than or equal to minimum salary.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortFields.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage("SortBy is not supported.");

        RuleFor(x => x.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction) || AllowedSortDirections.Contains(direction.Trim().ToLowerInvariant()))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
