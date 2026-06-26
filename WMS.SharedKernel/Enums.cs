namespace WMS.SharedKernel.Enums;

public enum PurchaseOrderStatus
{
    Draft,
    Confirmed,
    PartialReceived,
    FullyReceived,
    Cancelled
}

public enum GoodsReceiptStatus
{
    Draft,
    Completed,
    Cancelled
}

public enum SalesOrderStatus
{
    Draft,
    Confirmed,
    PartialPicked,
    FullyPicked,
    Shipped,
    Cancelled
}

public enum PickListStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public enum GoodsIssueStatus
{
    Draft,
    Completed,
    Cancelled
}

public enum TransferOrderStatus
{
    Draft,
    Approved,
    InTransit,
    Completed,
    Cancelled
}

public enum AdjustmentStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected
}

public enum MovementType
{
    Receipt,
    Issue,
    Transfer,
    Adjustment,
    Return
}

public enum ZoneType
{
    Receiving,
    Storage,
    Staging,
    Shipping,
    Return
}
