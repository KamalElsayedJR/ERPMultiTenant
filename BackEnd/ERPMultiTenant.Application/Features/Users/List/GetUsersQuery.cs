using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.List;

public sealed record GetUsersQuery : IRequest<Result<IReadOnlyList<UserListItemResponse>>>;
