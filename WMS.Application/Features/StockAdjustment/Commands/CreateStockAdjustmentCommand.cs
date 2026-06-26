using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.StockAdjustment.Commands;

public record CreateStockAdjustmentItemInput(Guid ProductId, Guid LocationId, decimal ActualQty);

public record CreateStockAdjustmentCommand(
    Guid WarehouseId,
    string? Reason,
    List<CreateStockAdjustmentItemInput> Items) : IRequest<Result<Guid>>;

public class CreateStockAdjustmentCommandValidator : AbstractValidator<CreateStockAdjustmentCommand>
{
    public CreateStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one adjustment item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            item.RuleFor(x => x.ActualQty)
                .GreaterThanOrEqualTo(0).WithMessage("Actual quantity must be greater than or equal to 0.");
        });
    }
}

public class CreateStockAdjustmentCommandHandler : IRequestHandler<CreateStockAdjustmentCommand, Result<Guid>>
{
    private readonly IStockAdjustmentRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateStockAdjustmentCommandHandler(
        IStockAdjustmentRepository repository,
        IStockRepository stockRepository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _stockRepository = stockRepository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var nextAdjNumber = await _repository.GetNextAdjustmentNumberAsync(cancellationToken);
        var createdBy = _currentUserService.UserId ?? Guid.Empty;

        var adjustment = new WMS.Domain.Entities.Inventory.StockAdjustment
        {
            Id = _idGenerator.Generate(),
            AdjNumber = nextAdjNumber,
            WarehouseId = request.WarehouseId,
            AdjustmentDate = DateTime.UtcNow,
            Reason = request.Reason,
            Status = AdjustmentStatus.Draft,
            CreatedBy = createdBy
        };

        foreach (var itemInput in request.Items)
        {
            // Lookup system quantity
            var stockRecords = await _stockRepository.FindAsync(
                s => s.ProductId == itemInput.ProductId && s.LocationId == itemInput.LocationId, 
                cancellationToken);

            var systemQty = stockRecords.FirstOrDefault()?.Quantity ?? 0m;
            var difference = itemInput.ActualQty - systemQty;

            var item = new StockAdjustmentItem
            {
                Id = _idGenerator.Generate(),
                StockAdjustmentId = adjustment.Id,
                ProductId = itemInput.ProductId,
                LocationId = itemInput.LocationId,
                SystemQty = systemQty,
                ActualQty = itemInput.ActualQty,
                Difference = difference
            };

            adjustment.Items.Add(item);
        }

        await _repository.AddAsync(adjustment, cancellationToken);

        return Result.Success(adjustment.Id);
    }
}
