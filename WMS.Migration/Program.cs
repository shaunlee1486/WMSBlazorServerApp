using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using DbUp;
using DbUp.Engine;

var connectionString = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=wms_db;Username=wms_user;Password=wms_pass;Timeout=15;CommandTimeout=30";

Console.WriteLine("Starting Database Migration using DbUp...");

try
{
    // Ensure the database exists
    EnsureDatabase.For.PostgresqlDatabase(connectionString);

    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isDevelopment = environment != null && environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

    // Run Scripts (DDL)
    RunFolderWithDbUp(connectionString, "Scripts");

    // Run SeedData
    RunFolderWithDbUp(connectionString, "SeedData");

    // Run SampleData if Development environment
    if (isDevelopment)
    {
        RunFolderWithDbUp(connectionString, "SampleData");
    }
    else
    {
        Console.WriteLine("[Migration] Skipping SampleData because ASPNETCORE_ENVIRONMENT is not 'Development'.");
    }

    Console.WriteLine("Database Migration completed successfully!");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Migration failed: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

static void RunFolderWithDbUp(string connStr, string folder)
{
    var path = GetFolderPath(folder);
    if (path == null)
    {
        Console.WriteLine($"[Warning] Folder not found: {folder}. Skipping...");
        return;
    }

    Console.WriteLine($"[Migration] Running DbUp for folder: {folder} from path {path}");
    var upgrader = DeployChanges.To
        .PostgresqlDatabase(connStr)
        .WithScriptsFromFileSystem(path)
        .LogToConsole()
        .Build();

    var result = upgrader.PerformUpgrade();
    if (!result.Successful)
    {
        throw new Exception($"DbUp migration failed for folder '{folder}': {result.Error}");
    }
}

static string? GetFolderPath(string folder)
{
    var path = Path.Combine(AppContext.BaseDirectory, folder);
    if (Directory.Exists(path)) return path;

    path = Path.Combine(Directory.GetCurrentDirectory(), folder);
    if (Directory.Exists(path)) return path;

    path = Path.Combine(Directory.GetCurrentDirectory(), "WMS.Migration", folder);
    if (Directory.Exists(path)) return path;

    return null;
}
