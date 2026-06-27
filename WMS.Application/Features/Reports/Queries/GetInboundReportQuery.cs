using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Reports.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Reports.Queries;

public record GetInboundReportQuery(Guid? SupplierId, DateTime? StartDate, DateTime? EndDate) : IRequest<Result<List<InboundReportDto>>>;

public class GetInboundReportQueryHandler : IRequestHandler<GetInboundReportQuery, Result<List<InboundReportDto>>>
{
    private readonly IGoodsReceiptRepository _receiptRepository;

    public GetInboundReportQueryHandler(IGoodsReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task<Result<List<InboundReportDto>>> Handle(GetInboundReportQuery request, CancellationToken cancellationToken)
    {
        var items = await _receiptRepository.GetInboundReportAsync(request.SupplierId, request.StartDate, request.EndDate, cancellationToken);
        
        var dtos = items.Select(gri => new InboundReportDto
        {
            GRNumber = gri.GoodsReceipt.GRNumber,
            SupplierName = gri.GoodsReceipt.PurchaseOrder.Supplier.Name,
            PONumber = gri.GoodsReceipt.PurchaseOrder.PONumber,
            ReceivedDate = gri.GoodsReceipt.ReceivedDate,
            ProductCode = gri.Product.Code,
            ProductName = gri.Product.Name,
            ReceivedQty = gri.ReceivedQty,
            LocationBarcode = gri.Location.Barcode ?? string.Empty,
            BatchNo = gri.BatchNo,
            ExpiryDate = gri.ExpiryDate
        }).ToList();

        return Result.Success(dtos);
    }
}
