using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.UserManagement.Commands;

public record CreateUserCommand(string Email, string Password, string FullName, List<string> Roles) : IRequest<Result<Guid>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure<Guid>("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<Guid>("Password is required.");
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure<Guid>("Full name is required.");

        return await _identityService.CreateUserAsync(request.Email, request.Password, request.FullName, request.Roles, cancellationToken);
    }
}
