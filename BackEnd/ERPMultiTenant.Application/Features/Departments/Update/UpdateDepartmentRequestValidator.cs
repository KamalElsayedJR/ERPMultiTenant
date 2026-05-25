using FluentValidation;

namespace ERPMultiTenant.Application.Features.Departments.Update;

public sealed class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(200).WithMessage("Department name must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be 500 characters or fewer.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
