using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Inbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.GoodsReceipts.Commands;

public record CreateGoodsReceiptItemInput(
    Guid ProductId,
    Guid LocationId,
    decimal ReceivedQty,
    string? BatchNo,
    DateTime? ExpiryDate);

public record CreateGoodsReceiptCommand(
    Guid PurchaseOrderId,
    string? Note,
    List<CreateGoodsReceiptItemInput> Items) : IRequest<Result<Guid>>;

public class CreateGoodsReceiptCommandValidator : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one received item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            item.RuleFor(x => x.ReceivedQty)
                .GreaterThan(0).WithMessage("Received quantity must be greater than 0.");
        });
    }
}

public class CreateGoodsReceiptCommandHandler : IRequestHandler<CreateGoodsReceiptCommand, Result<Guid>>
{
    private readonly IGoodsReceiptRepository _repository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateGoodsReceiptCommandHandler(
        IGoodsReceiptRepository repository,
        IPurchaseOrderRepository poRepository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _poRepository = poRepository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var po = await _poRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (po == null)
        {
            return Result.Failure<Guid>($"Purchase order with ID '{request.PurchaseOrderId}' was not found.");
        }

        if (po.Status != PurchaseOrderStatus.Confirmed && po.Status != PurchaseOrderStatus.PartialReceived)
        {
            return Result.Failure<Guid>($"Cannot receive goods against a purchase order with status '{po.Status}'.");
        }

        var nextGRNumber = await _repository.GetNextGRNumberAsync(cancellationToken);
        var receivedBy = _currentUserService.UserId ?? Guid.Empty;

        var gr = new GoodsReceipt
        {
            Id = _idGenerator.Generate(),
            GRNumber = nextGRNumber,
            PurchaseOrderId = request.PurchaseOrderId,
            ReceivedDate = DateTime.UtcNow,
            ReceivedBy = receivedBy,
            Status = GoodsReceiptStatus.Draft,
            Note = request.Note
        };

        foreach (var itemInput in request.Items)
        {
            var item = new GoodsReceiptItem
            {
                Id = _idGenerator.Generate(),
                GoodsReceiptId = gr.Id,
                ProductId = itemInput.ProductId,
                LocationId = itemInput.LocationId,
                ReceivedQty = itemInput.ReceivedQty,
                BatchNo = itemInput.BatchNo,
                ExpiryDate = itemInput.ExpiryDate
            };

            gr.Items.Add(item);
        }

        await _repository.AddAsync(gr, cancellationToken);

        return Result.Success(gr.Id);
    }
}
