using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Inbound;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IGoodsReceiptRepository : IRepository<GoodsReceipt>
{
    Task<GoodsReceipt?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<GoodsReceipt>> GetGoodsReceiptsPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextGRNumberAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoodsReceiptItem>> GetInboundReportAsync(Guid? supplierId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}
