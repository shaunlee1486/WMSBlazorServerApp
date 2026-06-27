using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Features.UserManagement.DTOs;
using WMS.Domain.Entities.Identity;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.UserManagement.Queries;

public record GetUsersQuery(string? SearchTerm, int Page, int PageSize) : IRequest<Result<PagedResult<UserDto>>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IRepository<AppUser> _userRepository;
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IRepository<AppUser> userRepository, IIdentityService identityService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<AppUser, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            predicate = u => u.Email!.ToLower().Contains(search) || u.FullName.ToLower().Contains(search);
        }

        var pagedUsers = await _userRepository.GetPagedAsync(
            predicate,
            "CreatedAt",
            "desc",
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = new List<UserDto>();
        foreach (var u in pagedUsers.Items)
        {
            var roles = await _identityService.GetUserRolesAsync(u.Id, cancellationToken);
            dtos.Add(new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                IsActive = u.IsActive,
                Roles = roles,
                CreatedAt = u.CreatedAt
            });
        }

        var result = new PagedResult<UserDto>(dtos, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PageSize);
        return Result.Success(result);
    }
}
