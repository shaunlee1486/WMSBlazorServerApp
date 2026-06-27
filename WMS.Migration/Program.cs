using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Npgsql;

var connectionString = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=wms_db;Username=wms_user;Password=wms_pass;Timeout=15;CommandTimeout=30";

if (args.Contains("hash-password"))
{
    var pwd = args.Length > 1 ? args[1] : "Admin123!";
    var hash = HashPassword(pwd);
    Console.WriteLine($"Password: {pwd}");
    Console.WriteLine($"Hash: {hash}");
    return;
}

Console.WriteLine("Starting Database Migration...");
Console.WriteLine($"Connection string target database: {new NpgsqlConnectionStringBuilder(connectionString).Database}");

try
{
    // Try to connect and ensure target database exists
    await EnsureDatabaseExists(connectionString);

    // Ensure migration history tracking table exists
    await EnsureMigrationTableExists(connectionString);

    // Run Scripts (DDL)
    await RunFolder(connectionString, "Scripts");
    // Run SeedData
    await RunFolder(connectionString, "SeedData");
    // Run SampleData
    await RunFolder(connectionString, "SampleData");

    Console.WriteLine("Database Migration completed successfully!");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Migration failed: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

static async Task EnsureDatabaseExists(string connStr)
{
    var builder = new NpgsqlConnectionStringBuilder(connStr);
    var dbName = builder.Database;
    
    // Connect to default 'postgres' database to check and create target database
    builder.Database = "postgres";
    var systemConnStr = builder.ConnectionString;

    using var conn = new NpgsqlConnection(systemConnStr);
    await conn.OpenAsync();

    using var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = @dbName", conn);
    cmd.Parameters.AddWithValue("dbName", dbName ?? "wms_db");
    var exists = await cmd.ExecuteScalarAsync();

    if (exists == null)
    {
        Console.WriteLine($"Database '{dbName}' does not exist. Creating...");
        // Command parameters cannot be used for database name identifiers in CREATE DATABASE statements.
        // It's safe here as dbName is extracted from connection string which is developer-controlled.
        using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", conn);
        await createCmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Database '{dbName}' created.");
    }
}

static async Task EnsureMigrationTableExists(string connStr)
{
    using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();

    var sql = @"
        CREATE TABLE IF NOT EXISTS ""MigrationHistory"" (
            ""Id"" UUID PRIMARY KEY,
            ""ScriptName"" VARCHAR(255) NOT NULL UNIQUE,
            ""Hash"" VARCHAR(255) NOT NULL,
            ""ExecutedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
        );";

    using var cmd = new NpgsqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync();
}

static async Task RunFolder(string connStr, string folder)
{
    // Search order for the folders:
    // 1. AppContext Base Directory
    // 2. Current Directory
    // 3. Current Directory / WMS.Migration
    
    var path = Path.Combine(AppContext.BaseDirectory, folder);
    
    if (!Directory.Exists(path))
    {
        path = Path.Combine(Directory.GetCurrentDirectory(), folder);
    }
    
    if (!Directory.Exists(path))
    {
        path = Path.Combine(Directory.GetCurrentDirectory(), "WMS.Migration", folder);
    }

    if (!Directory.Exists(path))
    {
        Console.WriteLine($"[Warning] Folder not found: {folder}. Skipping...");
        return;
    }

    Console.WriteLine($"[Migration] Running folder: {folder} from path {path}");
    var files = Directory.GetFiles(path, "*.sql").OrderBy(f => f).ToList();

    if (!files.Any())
    {
        Console.WriteLine($"[Migration] No SQL scripts found in {folder}");
        return;
    }

    using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();

    foreach (var file in files)
    {
        var fileName = Path.GetFileName(file);
        
        // Check if script has already been run
        using var checkCmd = new NpgsqlCommand(@"SELECT 1 FROM ""MigrationHistory"" WHERE ""ScriptName"" = @name", conn);
        checkCmd.Parameters.AddWithValue("name", fileName);
        var exists = await checkCmd.ExecuteScalarAsync();

        if (exists != null)
        {
            Console.WriteLine($"[Migration] Script {fileName} already executed. Skipping.");
            continue;
        }

        Console.WriteLine($"[Migration] Executing script: {fileName}");

        var scriptContent = await File.ReadAllTextAsync(file);

        // Compute hash
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(scriptContent));
        var hashString = Convert.ToHexString(hashBytes);

        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            using var cmd = new NpgsqlCommand(scriptContent, conn, transaction);
            await cmd.ExecuteNonQueryAsync();

            // Insert into history table
            using var insertCmd = new NpgsqlCommand(@"
                INSERT INTO ""MigrationHistory"" (""Id"", ""ScriptName"", ""Hash"", ""ExecutedAt"")
                VALUES (@id, @name, @hash, @date)", conn, transaction);
            insertCmd.Parameters.AddWithValue("id", Guid.NewGuid());
            insertCmd.Parameters.AddWithValue("name", fileName);
            insertCmd.Parameters.AddWithValue("hash", hashString);
            insertCmd.Parameters.AddWithValue("date", DateTime.UtcNow);
            await insertCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            Console.WriteLine($"[Migration] Script {fileName} completed.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] Script {fileName} failed: {ex.Message}");
            Console.ResetColor();
            throw;
        }
    }
}

static string HashPassword(string password)
{
    byte[] salt = new byte[16];
    RandomNumberGenerator.Fill(salt);
    byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
    
    var outputBytes = new byte[1 + 4 + 4 + 4 + 16 + 32];
    outputBytes[0] = 0x01; // format marker
    
    // Write PRF (0 = HMAC-SHA256)
    WriteNetworkByteOrder(outputBytes, 1, 0);
    // Write iteration count (100000)
    WriteNetworkByteOrder(outputBytes, 5, 100000);
    // Write salt size (16)
    WriteNetworkByteOrder(outputBytes, 9, 16);
    // Write salt
    Buffer.BlockCopy(salt, 0, outputBytes, 13, 16);
    // Write subkey
    Buffer.BlockCopy(subkey, 0, outputBytes, 29, 32);
    
    return Convert.ToBase64String(outputBytes);
}

static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
{
    buffer[offset + 0] = (byte)(value >> 24);
    buffer[offset + 1] = (byte)(value >> 16);
    buffer[offset + 2] = (byte)(value >> 8);
    buffer[offset + 3] = (byte)(value);
}
