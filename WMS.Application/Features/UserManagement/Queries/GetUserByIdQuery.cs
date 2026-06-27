using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Features.UserManagement.DTOs;
using WMS.Domain.Entities.Identity;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.UserManagement.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IRepository<AppUser> _userRepository;
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IRepository<AppUser> userRepository, IIdentityService identityService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UserDto>($"User with ID '{request.Id}' was not found.");
        }

        var roles = await _identityService.GetUserRolesAsync(user.Id, cancellationToken);
        
        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };

        return Result.Success(dto);
    }
}
