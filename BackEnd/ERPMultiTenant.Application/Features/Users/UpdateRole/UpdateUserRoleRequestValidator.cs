using FluentValidation;

namespace ERPMultiTenant.Application.Features.Users.UpdateRole;

public sealed class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is not valid.");
    }
}
