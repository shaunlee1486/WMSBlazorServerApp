using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.UserManagement.Commands;

public record ResetUserPasswordCommand(Guid Id, string NewPassword) : IRequest<Result>;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetUserPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return Result.Failure("New password is required.");
        if (request.NewPassword.Length < 8)
            return Result.Failure("Password must be at least 8 characters long.");

        return await _identityService.ResetPasswordAsync(request.Id, request.NewPassword, cancellationToken);
    }
}
