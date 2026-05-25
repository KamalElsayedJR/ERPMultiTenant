using FluentValidation;

namespace ERPMultiTenant.Application.Features.Employees.Create;

public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.JobTitle)
            .MaximumLength(150).WithMessage("Job title must be 150 characters or fewer.")
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required.");

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0).WithMessage("Salary must be non-negative.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required.");

        RuleFor(x => x.ApplicationUserId)
            .NotEmpty().WithMessage("Application user is required.");
    }
}
