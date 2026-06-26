using System;
using Microsoft.AspNetCore.Identity;

namespace WMS.Domain.Entities.Identity;

public class AppRole : IdentityRole<Guid>
{
    public AppRole() : base()
    {
    }

    public AppRole(string roleName) : base(roleName)
    {
    }

    public AppRole(string roleName, string description) : base(roleName)
    {
        Description = description;
    }

    public string Description { get; set; } = string.Empty;
}
