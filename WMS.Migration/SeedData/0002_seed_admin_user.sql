-- 0002_seed_admin_user.sql

INSERT INTO "Users" (
    "Id", 
    "UserName", 
    "NormalizedUserName", 
    "Email", 
    "NormalizedEmail", 
    "EmailConfirmed", 
    "PasswordHash", 
    "SecurityStamp", 
    "ConcurrencyStamp", 
    "PhoneNumberConfirmed", 
    "TwoFactorEnabled", 
    "LockoutEnabled", 
    "AccessFailedCount", 
    "FullName", 
    "AvatarPath", 
    "IsActive", 
    "CreatedAt"
) VALUES (
    'a0f0d2c3-9bfa-48ef-93a0-fcd84ff100d8',
    'admin',
    'ADMIN',
    'admin@wms.com',
    'ADMIN@WMS.COM',
    TRUE,
    'AQAAAAAAAYagAAAAEHA5ybkBsPgLOi/KjgP4nZQeF7neoCXal3eonhCoGDkS9V40/2r6xW4kKW1NAu5WSA==',
    '4b7b3b9b-9bfb-48ef-93a0-fcd84ff100d8',
    '8fb6299b-4bfb-48ef-93a0-fcd84ff100d8',
    FALSE,
    FALSE,
    TRUE,
    0,
    'System Administrator',
    NULL,
    TRUE,
    CURRENT_TIMESTAMP
)
ON CONFLICT ("Id") DO UPDATE SET
"PasswordHash" = EXCLUDED."PasswordHash",
"FullName" = EXCLUDED."FullName",
"IsActive" = EXCLUDED."IsActive";

-- Assign Admin Role (Admin Role ID = d011f017-7422-4809-9f7b-99f6ec36e6ba)
INSERT INTO "UserRoles" ("UserId", "RoleId")
VALUES ('a0f0d2c3-9bfa-48ef-93a0-fcd84ff100d8', 'd011f017-7422-4809-9f7b-99f6ec36e6ba')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;
