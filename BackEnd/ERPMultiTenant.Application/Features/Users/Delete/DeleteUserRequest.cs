using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.Delete;

public sealed record DeleteUserRequest(Guid UserId) : IRequest<Result<DeleteUserResponse>>;
