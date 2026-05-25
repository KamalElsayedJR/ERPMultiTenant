using FluentValidation;

namespace ERPMultiTenant.Application.Features.Employees.Update;

public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee id is required.");

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required.")
            .MaximumLength(50).WithMessage("Employee number must be 50 characters or fewer.");

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
