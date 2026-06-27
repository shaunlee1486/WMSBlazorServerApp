using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WMS.SharedKernel;

namespace WMS.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password, string fullName, List<string> roles, CancellationToken cancellationToken = default);
    
    Task<Result> UpdateUserAsync(Guid id, string email, string fullName, bool isActive, List<string> roles, CancellationToken cancellationToken = default);
    
    Task<Result> ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default);
    
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
}
