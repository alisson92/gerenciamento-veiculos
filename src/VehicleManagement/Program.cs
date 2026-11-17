using Microsoft.Data.SqlClient;
using System.Text.Json;
using VehicleManagement.Data;
using VehicleManagement.UI;

// Reads the connection string from appsettings.json (copied next to the executable).
// It can be overridden with the environment variable ConnectionStrings__VehicleDb.
string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__VehicleDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    using var json = JsonDocument.Parse(File.ReadAllText(settingsPath));
    connectionString = json.RootElement
        .GetProperty("ConnectionStrings")
        .GetProperty("VehicleDb")
        .GetString();
}

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'VehicleDb' not found.");

var repository = new VehicleRepository(connectionString);

try
{
    repository.TestConnection();
}
catch (SqlException ex)
{
    Console.WriteLine("Could not connect to SQL Server / database.");
    Console.WriteLine($"Details: {ex.Message}");
    Console.WriteLine("\nCheck: 1) the SQL Server service is running, 2) database/Vehicles.sql was executed,");
    Console.WriteLine("       3) the connection string in appsettings.json is correct (see docs/RUNBOOK.md).");
    return 1;
}

new VehicleMenu(repository).Run();
Console.WriteLine("Goodbye!");
return 0;
