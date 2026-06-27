using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Identity;
using WMS.SharedKernel;

namespace WMS.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public IdentityService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<Guid>> CreateUserAsync(string email, string password, string fullName, List<string> roles, CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result.Failure<Guid>(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (roles != null && roles.Any())
        {
            var roleResult = await _userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return Result.Failure<Guid>(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        return Result.Success(user.Id);
    }

    public async Task<Result> UpdateUserAsync(Guid id, string email, string fullName, bool isActive, List<string> roles, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Result.Failure($"User with ID '{id}' not found.");
        }

        user.Email = email;
        user.UserName = email;
        user.FullName = fullName;
        user.IsActive = isActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roles).ToList();
        var rolesToAdd = roles.Except(currentRoles).ToList();

        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return Result.Failure(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
            }
        }

        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return Result.Failure(string.Join("; ", addResult.Errors.Select(e => e.Description)));
            }
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Result.Failure($"User with ID '{id}' not found.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return Result.Success();
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
        return roles;
    }
}
