using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.StockAdjustment.Commands;

public record UpdateStockAdjustmentCommand(
    Guid Id,
    string? Reason,
    List<CreateStockAdjustmentItemInput> Items) : IRequest<Result>;

public class UpdateStockAdjustmentCommandValidator : AbstractValidator<UpdateStockAdjustmentCommand>
{
    public UpdateStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Adjustment ID is required.");

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

public class UpdateStockAdjustmentCommandHandler : IRequestHandler<UpdateStockAdjustmentCommand, Result>
{
    private readonly IStockAdjustmentRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IIdGenerator _idGenerator;

    public UpdateStockAdjustmentCommandHandler(
        IStockAdjustmentRepository repository,
        IStockRepository stockRepository,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _stockRepository = stockRepository;
        _idGenerator = idGenerator;
    }

    public async Task<Result> Handle(UpdateStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var sa = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (sa == null)
        {
            return Result.Failure($"Stock adjustment with ID '{request.Id}' was not found.");
        }

        if (sa.Status != AdjustmentStatus.Draft)
        {
            return Result.Failure($"Only draft stock adjustments can be updated. Current status: {sa.Status}");
        }

        sa.Reason = request.Reason;
        sa.AdjustmentDate = DateTime.UtcNow;

        // Clear existing items
        sa.Items.Clear();

        // Add new items
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
                StockAdjustmentId = sa.Id,
                ProductId = itemInput.ProductId,
                LocationId = itemInput.LocationId,
                SystemQty = systemQty,
                ActualQty = itemInput.ActualQty,
                Difference = difference
            };

            sa.Items.Add(item);
        }

        _repository.Update(sa);

        return Result.Success();
    }
}
