-- 0005_create_outbound_tables.sql

CREATE TABLE IF NOT EXISTS "SalesOrders" (
    "Id" UUID PRIMARY KEY,
    "SONumber" VARCHAR(50) NOT NULL UNIQUE,
    "CustomerId" UUID NOT NULL REFERENCES "Customers"("Id") ON DELETE RESTRICT,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Draft, Confirmed, PartialPicked, FullyPicked, Shipped, Cancelled
    "OrderDate" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "RequiredDate" TIMESTAMPTZ NULL,
    "Note" TEXT NULL,
    "CreatedBy" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "SalesOrderItems" (
    "Id" UUID PRIMARY KEY,
    "SalesOrderId" UUID NOT NULL REFERENCES "SalesOrders"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "OrderedQty" DECIMAL(18,4) NOT NULL,
    "PickedQty" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "UnitPrice" DECIMAL(18,4) NOT NULL
);

CREATE TABLE IF NOT EXISTS "PickLists" (
    "Id" UUID PRIMARY KEY,
    "PickListNumber" VARCHAR(50) NOT NULL UNIQUE,
    "SalesOrderId" UUID NOT NULL REFERENCES "SalesOrders"("Id") ON DELETE RESTRICT,
    "AssignedTo" UUID NULL REFERENCES "Users"("Id") ON DELETE SET NULL,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Pending, InProgress, Completed, Cancelled
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "PickListItems" (
    "Id" UUID PRIMARY KEY,
    "PickListId" UUID NOT NULL REFERENCES "PickLists"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "LocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "RequiredQty" DECIMAL(18,4) NOT NULL,
    "PickedQty" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "Status" VARCHAR(50) NOT NULL -- e.g., Pending, Completed
);

CREATE TABLE IF NOT EXISTS "GoodsIssues" (
    "Id" UUID PRIMARY KEY,
    "GINumber" VARCHAR(50) NOT NULL UNIQUE,
    "SalesOrderId" UUID NOT NULL REFERENCES "SalesOrders"("Id") ON DELETE RESTRICT,
    "IssuedDate" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IssuedBy" UUID NOT NULL,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Draft, Completed, Cancelled
    "Note" TEXT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "GoodsIssueItems" (
    "Id" UUID PRIMARY KEY,
    "GoodsIssueId" UUID NOT NULL REFERENCES "GoodsIssues"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "LocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "IssuedQty" DECIMAL(18,4) NOT NULL,
    "BatchNo" VARCHAR(100) NULL
);
