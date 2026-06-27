-- 0007_add_performance_indexes.sql

-- Index on StockMovements
CREATE INDEX IF NOT EXISTS "ix_stockmovements_productid" ON "StockMovements"("ProductId");
CREATE INDEX IF NOT EXISTS "ix_stockmovements_fromlocationid" ON "StockMovements"("FromLocationId");
CREATE INDEX IF NOT EXISTS "ix_stockmovements_tolocationid" ON "StockMovements"("ToLocationId");
CREATE INDEX IF NOT EXISTS "ix_stockmovements_createdat" ON "StockMovements"("CreatedAt");
CREATE INDEX IF NOT EXISTS "ix_stockmovements_movementtype" ON "StockMovements"("MovementType");

-- Index on StockAdjustments
CREATE INDEX IF NOT EXISTS "ix_stockadjustments_warehouseid" ON "StockAdjustments"("WarehouseId");
CREATE INDEX IF NOT EXISTS "ix_stockadjustments_status" ON "StockAdjustments"("Status");
CREATE INDEX IF NOT EXISTS "ix_stockadjustments_createdat" ON "StockAdjustments"("CreatedAt");

-- Index on StockAdjustmentItems
CREATE INDEX IF NOT EXISTS "ix_stockadjustmentitems_stockadjustmentid" ON "StockAdjustmentItems"("StockAdjustmentId");
CREATE INDEX IF NOT EXISTS "ix_stockadjustmentitems_productid" ON "StockAdjustmentItems"("ProductId");
CREATE INDEX IF NOT EXISTS "ix_stockadjustmentitems_locationid" ON "StockAdjustmentItems"("LocationId");

-- Index on PurchaseOrders
CREATE INDEX IF NOT EXISTS "ix_purchaseorders_supplierid" ON "PurchaseOrders"("SupplierId");
CREATE INDEX IF NOT EXISTS "ix_purchaseorders_status" ON "PurchaseOrders"("Status");
CREATE INDEX IF NOT EXISTS "ix_purchaseorders_createdat" ON "PurchaseOrders"("CreatedAt");

-- Index on PurchaseOrderItems
CREATE INDEX IF NOT EXISTS "ix_purchaseorderitems_purchaseorderid" ON "PurchaseOrderItems"("PurchaseOrderId");
CREATE INDEX IF NOT EXISTS "ix_purchaseorderitems_productid" ON "PurchaseOrderItems"("ProductId");

-- Index on GoodsReceipts
CREATE INDEX IF NOT EXISTS "ix_goodsreceipts_purchaseorderid" ON "GoodsReceipts"("PurchaseOrderId");
CREATE INDEX IF NOT EXISTS "ix_goodsreceipts_status" ON "GoodsReceipts"("Status");
CREATE INDEX IF NOT EXISTS "ix_goodsreceipts_receiveddate" ON "GoodsReceipts"("ReceivedDate");

-- Index on GoodsReceiptItems
CREATE INDEX IF NOT EXISTS "ix_goodsreceiptitems_goodsreceiptid" ON "GoodsReceiptItems"("GoodsReceiptId");
CREATE INDEX IF NOT EXISTS "ix_goodsreceiptitems_productid" ON "GoodsReceiptItems"("ProductId");
CREATE INDEX IF NOT EXISTS "ix_goodsreceiptitems_locationid" ON "GoodsReceiptItems"("LocationId");

-- Index on SalesOrders
CREATE INDEX IF NOT EXISTS "ix_salesorders_customerid" ON "SalesOrders"("CustomerId");
CREATE INDEX IF NOT EXISTS "ix_salesorders_status" ON "SalesOrders"("Status");
CREATE INDEX IF NOT EXISTS "ix_salesorders_createdat" ON "SalesOrders"("CreatedAt");

-- Index on SalesOrderItems
CREATE INDEX IF NOT EXISTS "ix_salesorderitems_salesorderid" ON "SalesOrderItems"("SalesOrderId");
CREATE INDEX IF NOT EXISTS "ix_salesorderitems_productid" ON "SalesOrderItems"("ProductId");

-- Index on PickLists
CREATE INDEX IF NOT EXISTS "ix_picklists_salesorderid" ON "PickLists"("SalesOrderId");
CREATE INDEX IF NOT EXISTS "ix_picklists_status" ON "PickLists"("Status");
CREATE INDEX IF NOT EXISTS "ix_picklists_createdat" ON "PickLists"("CreatedAt");

-- Index on PickListItems
CREATE INDEX IF NOT EXISTS "ix_picklistitems_picklistid" ON "PickListItems"("PickListId");
CREATE INDEX IF NOT EXISTS "ix_picklistitems_productid" ON "PickListItems"("ProductId");
CREATE INDEX IF NOT EXISTS "ix_picklistitems_locationid" ON "PickListItems"("LocationId");

-- Index on GoodsIssues
CREATE INDEX IF NOT EXISTS "ix_goodsissues_salesorderid" ON "GoodsIssues"("SalesOrderId");
CREATE INDEX IF NOT EXISTS "ix_goodsissues_status" ON "GoodsIssues"("Status");
CREATE INDEX IF NOT EXISTS "ix_goodsissues_issueddate" ON "GoodsIssues"("IssuedDate");

-- Index on GoodsIssueItems
CREATE INDEX IF NOT EXISTS "ix_goodsissueitems_goodsissueid" ON "GoodsIssueItems"("GoodsIssueId");
CREATE INDEX IF NOT EXISTS "ix_goodsissueitems_productid" ON "GoodsIssueItems"("ProductId");
CREATE INDEX IF NOT EXISTS "ix_goodsissueitems_locationid" ON "GoodsIssueItems"("LocationId");

-- Index on TransferOrders
CREATE INDEX IF NOT EXISTS "ix_transferorders_fromwarehouseid" ON "TransferOrders"("FromWarehouseId");
CREATE INDEX IF NOT EXISTS "ix_transferorders_towarehouseid" ON "TransferOrders"("ToWarehouseId");
CREATE INDEX IF NOT EXISTS "ix_transferorders_status" ON "TransferOrders"("Status");
CREATE INDEX IF NOT EXISTS "ix_transferorders_createdat" ON "TransferOrders"("CreatedAt");

-- Index on TransferOrderItems
CREATE INDEX IF NOT EXISTS "ix_transferorderitems_transferorderid" ON "TransferOrderItems"("TransferOrderId");
CREATE INDEX IF NOT EXISTS "ix_transferorderitems_productid" ON "TransferOrderItems"("ProductId");

-- Index on Returns
CREATE INDEX IF NOT EXISTS "ix_returns_status" ON "Returns"("Status");
CREATE INDEX IF NOT EXISTS "ix_returns_createdat" ON "Returns"("CreatedAt");

-- Index on ReturnItems
CREATE INDEX IF NOT EXISTS "ix_returnitems_returnid" ON "ReturnItems"("ReturnId");
CREATE INDEX IF NOT EXISTS "ix_returnitems_productid" ON "ReturnItems"("ProductId");
CREATE INDEX IF NOT EXISTS "ix_returnitems_locationid" ON "ReturnItems"("LocationId");

-- Index on AuditLogs
CREATE INDEX IF NOT EXISTS "ix_auditlogs_timestamp" ON "AuditLogs"("Timestamp");
CREATE INDEX IF NOT EXISTS "ix_auditlogs_userid" ON "AuditLogs"("UserId");
CREATE INDEX IF NOT EXISTS "ix_auditlogs_entityname" ON "AuditLogs"("EntityName");
