using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.UserManagement.Commands;

public record UpdateUserCommand(Guid Id, string Email, string FullName, bool IsActive, List<string> Roles) : IRequest<Result>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IIdentityService _identityService;

    public UpdateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure("Email is required.");
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure("Full name is required.");

        return await _identityService.UpdateUserAsync(request.Id, request.Email, request.FullName, request.IsActive, request.Roles, cancellationToken);
    }
}
