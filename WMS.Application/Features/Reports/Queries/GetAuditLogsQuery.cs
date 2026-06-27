using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Reports.DTOs;
using WMS.Domain.Entities.Identity;
using WMS.Domain.Entities.Reporting;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Reports.Queries;

public record GetAuditLogsQuery(
    string? EntityName,
    string? Action,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? UserId,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<PagedResult<AuditLogReportDto>>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogReportDto>>>
{
    private readonly IRepository<AuditLog> _auditLogRepository;
    private readonly IRepository<AppUser> _userRepository;

    public GetAuditLogsQueryHandler(IRepository<AuditLog> auditLogRepository, IRepository<AppUser> userRepository)
    {
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<AuditLogReportDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<AuditLog, bool>> predicate = x =>
            (string.IsNullOrEmpty(request.EntityName) || x.EntityName == request.EntityName) &&
            (string.IsNullOrEmpty(request.Action) || x.Action == request.Action) &&
            (!request.StartDate.HasValue || x.Timestamp >= request.StartDate.Value.ToUniversalTime()) &&
            (!request.EndDate.HasValue || x.Timestamp <= request.EndDate.Value.ToUniversalTime()) &&
            (!request.UserId.HasValue || x.UserId == request.UserId.Value);

        var pagedLogs = await _auditLogRepository.GetPagedAsync(
            predicate,
            "Timestamp",
            "desc",
            request.Page,
            request.PageSize,
            cancellationToken);

        var users = await _userRepository.GetAllAsync(cancellationToken);
        var userEmails = users.ToDictionary(u => u.Id, u => u.Email ?? string.Empty);

        var dtos = pagedLogs.Items.Select(x =>
        {
            string email = "System";
            if (x.UserId.HasValue)
            {
                userEmails.TryGetValue(x.UserId.Value, out var val);
                email = val ?? "Unknown";
            }

            return new AuditLogReportDto
            {
                Id = x.Id,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Action = x.Action,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                Timestamp = x.Timestamp,
                UserEmail = email
            };
        }).ToList();

        var result = new PagedResult<AuditLogReportDto>(dtos, pagedLogs.TotalCount, pagedLogs.Page, pagedLogs.PageSize);
        return Result.Success(result);
    }
}
