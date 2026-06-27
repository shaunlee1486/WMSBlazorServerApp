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

public record GetOutboundReportQuery(Guid? CustomerId, DateTime? StartDate, DateTime? EndDate) : IRequest<Result<List<OutboundReportDto>>>;

public class GetOutboundReportQueryHandler : IRequestHandler<GetOutboundReportQuery, Result<List<OutboundReportDto>>>
{
    private readonly IGoodsIssueRepository _issueRepository;

    public GetOutboundReportQueryHandler(IGoodsIssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<Result<List<OutboundReportDto>>> Handle(GetOutboundReportQuery request, CancellationToken cancellationToken)
    {
        var items = await _issueRepository.GetOutboundReportAsync(request.CustomerId, request.StartDate, request.EndDate, cancellationToken);
        
        var dtos = items.Select(gii => new OutboundReportDto
        {
            GINumber = gii.GoodsIssue.GINumber,
            CustomerName = gii.GoodsIssue.SalesOrder.Customer.Name,
            SONumber = gii.GoodsIssue.SalesOrder.SONumber,
            IssuedDate = gii.GoodsIssue.IssuedDate,
            ProductCode = gii.Product.Code,
            ProductName = gii.Product.Name,
            IssuedQty = gii.IssuedQty,
            LocationBarcode = gii.Location.Barcode ?? string.Empty,
            BatchNo = gii.BatchNo
        }).ToList();

        return Result.Success(dtos);
    }
}
