using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Returns.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Returns.Queries;

public record GetReturnByIdQuery(Guid Id) : IRequest<Result<ReturnDto>>;

public class GetReturnByIdQueryHandler : IRequestHandler<GetReturnByIdQuery, Result<ReturnDto>>
{
    private readonly IReturnRepository _repository;

    public GetReturnByIdQueryHandler(IReturnRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ReturnDto>> Handle(GetReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (r == null)
        {
            return Result.Failure<ReturnDto>($"Return with ID '{request.Id}' was not found.");
        }

        var dto = new ReturnDto
        {
            Id = r.Id,
            ReturnNumber = r.ReturnNumber,
            ReturnType = r.ReturnType.ToString(),
            ReferenceNo = r.ReferenceNo,
            Status = r.Status.ToString(),
            Note = r.Note,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            Items = r.Items.Select(ri => new ReturnItemDto
            {
                Id = ri.Id,
                ProductId = ri.ProductId,
                ProductCode = ri.Product.Code,
                ProductName = ri.Product.Name,
                LocationId = ri.LocationId,
                LocationBarcode = ri.Location.Barcode ?? string.Empty,
                Quantity = ri.Quantity,
                InspectionStatus = ri.InspectionStatus.ToString(),
                Note = ri.Note
            }).ToList()
        };

        return Result.Success(dto);
    }
}
