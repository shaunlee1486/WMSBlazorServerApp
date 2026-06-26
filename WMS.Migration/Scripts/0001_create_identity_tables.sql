-- 0001_create_identity_tables.sql

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY,
    "UserName" VARCHAR(256) NULL,
    "NormalizedUserName" VARCHAR(256) NULL UNIQUE,
    "Email" VARCHAR(256) NULL,
    "NormalizedEmail" VARCHAR(256) NULL UNIQUE,
    "EmailConfirmed" BOOLEAN NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL,
    "TwoFactorEnabled" BOOLEAN NOT NULL,
    "LockoutEnd" TIMESTAMPTZ NULL,
    "LockoutEnabled" BOOLEAN NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    "FullName" VARCHAR(200) NOT NULL DEFAULT '',
    "AvatarPath" VARCHAR(500) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Roles" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(256) NULL,
    "NormalizedName" VARCHAR(256) NULL UNIQUE,
    "ConcurrencyStamp" TEXT NULL,
    "Description" VARCHAR(500) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS "UserRoles" (
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "RoleId" UUID NOT NULL REFERENCES "Roles"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("UserId", "RoleId")
);

CREATE TABLE IF NOT EXISTS "UserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL
);

CREATE TABLE IF NOT EXISTS "RoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" UUID NOT NULL REFERENCES "Roles"("Id") ON DELETE CASCADE,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL
);

CREATE TABLE IF NOT EXISTS "UserLogins" (
    "LoginProvider" VARCHAR(128) NOT NULL,
    "ProviderKey" VARCHAR(128) NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("LoginProvider", "ProviderKey")
);

CREATE TABLE IF NOT EXISTS "UserTokens" (
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "LoginProvider" VARCHAR(128) NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "Value" TEXT NULL,
    PRIMARY KEY ("UserId", "LoginProvider", "Name")
);
