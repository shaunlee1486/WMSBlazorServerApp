using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PickList.Commands;

public record CompletePickListCommand(Guid Id) : IRequest<Result>;

public class CompletePickListCommandHandler : IRequestHandler<CompletePickListCommand, Result>
{
    private readonly IPickListRepository _repository;
    private readonly ISalesOrderRepository _soRepository;
    private readonly IGoodsIssueRepository _giRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CompletePickListCommandHandler(
        IPickListRepository repository,
        ISalesOrderRepository soRepository,
        IGoodsIssueRepository giRepository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _soRepository = soRepository;
        _giRepository = giRepository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CompletePickListCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Pick List
        var pl = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (pl == null)
        {
            return Result.Failure($"Pick list with ID '{request.Id}' was not found.");
        }

        if (pl.Status != PickListStatus.InProgress)
        {
            return Result.Failure($"Only pick lists in progress can be completed. Current status: {pl.Status}");
        }

        // 2. Fetch Sales Order
        var so = await _soRepository.GetByIdWithItemsAsync(pl.SalesOrderId, cancellationToken);
        if (so == null)
        {
            return Result.Failure($"Linked sales order with ID '{pl.SalesOrderId}' was not found.");
        }

        // 3. Update Sales Order item picked quantities
        foreach (var plItem in pl.Items)
        {
            var soItem = so.Items.FirstOrDefault(i => i.ProductId == plItem.ProductId);
            if (soItem != null)
            {
                // We add the picked quantity from the pick list
                soItem.PickedQty += plItem.PickedQty;
            }
        }

        // 4. Update Sales Order status
        var allFullyPicked = so.Items.All(i => i.PickedQty >= i.OrderedQty);
        var anyPicked = so.Items.Any(i => i.PickedQty > 0);

        if (allFullyPicked)
        {
            so.Status = SalesOrderStatus.FullyPicked;
        }
        else if (anyPicked)
        {
            so.Status = SalesOrderStatus.PartialPicked;
        }

        _soRepository.Update(so);

        // 5. Create Draft Goods Issue (only if at least some quantity was picked)
        var totalPicked = pl.Items.Sum(i => i.PickedQty);
        if (totalPicked > 0)
        {
            var nextGINumber = await _giRepository.GetNextGINumberAsync(cancellationToken);
            var issuedBy = _currentUserService.UserId ?? Guid.Empty;

            var gi = new WMS.Domain.Entities.Outbound.GoodsIssue
            {
                Id = _idGenerator.Generate(),
                GINumber = nextGINumber,
                SalesOrderId = so.Id,
                IssuedDate = DateTime.UtcNow,
                IssuedBy = issuedBy,
                Status = GoodsIssueStatus.Draft,
                Note = $"Goods Issue automatically generated from Pick List {pl.PickListNumber}"
            };

            foreach (var plItem in pl.Items.Where(i => i.PickedQty > 0))
            {
                var giItem = new WMS.Domain.Entities.Outbound.GoodsIssueItem
                {
                    Id = _idGenerator.Generate(),
                    GoodsIssueId = gi.Id,
                    ProductId = plItem.ProductId,
                    LocationId = plItem.LocationId,
                    IssuedQty = plItem.PickedQty,
                    BatchNo = null // Batch can be assigned in GI page if needed
                };
                gi.Items.Add(giItem);
            }

            await _giRepository.AddAsync(gi, cancellationToken);
        }

        // 6. Set Pick List status to Completed
        pl.Status = PickListStatus.Completed;
        _repository.Update(pl);

        return Result.Success();
    }
}
