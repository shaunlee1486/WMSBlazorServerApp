using System;

namespace WMS.Domain.Entities.Reporting;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g., Create, Update, Delete
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
