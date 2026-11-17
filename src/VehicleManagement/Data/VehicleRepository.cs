using Microsoft.Data.SqlClient;
using VehicleManagement.Models;

namespace VehicleManagement.Data;

/// <summary>
/// All database access lives here. Every query is parameterized
/// (no string concatenation), which prevents SQL injection.
/// </summary>
public class VehicleRepository
{
    private readonly string _connectionString;

    public VehicleRepository(string connectionString) => _connectionString = connectionString;

    /// <summary>Opens a connection just to verify the server/database is reachable.</summary>
    public void TestConnection()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
    }

    // ---------- CREATE ----------
    public int Create(Vehicle vehicle)
    {
        const string sql = @"
            INSERT INTO dbo.Vehicles (Make, Model, LicensePlate, ManufactureYear)
            OUTPUT INSERTED.Id
            VALUES (@Make, @Model, @LicensePlate, @ManufactureYear);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, vehicle);
        connection.Open();
        return (int)command.ExecuteScalar()!;
    }

    // ---------- READ ----------
    public List<Vehicle> GetAll()
    {
        const string sql = @"
            SELECT Id, Make, Model, LicensePlate, ManufactureYear
            FROM dbo.Vehicles
            ORDER BY Id;";

        var vehicles = new List<Vehicle>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            vehicles.Add(Map(reader));
        return vehicles;
    }

    public Vehicle? GetById(int id)
    {
        const string sql = @"
            SELECT Id, Make, Model, LicensePlate, ManufactureYear
            FROM dbo.Vehicles
            WHERE Id = @Id;";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // ---------- UPDATE ----------
    /// <returns>true if a row was updated.</returns>
    public bool Update(Vehicle vehicle)
    {
        const string sql = @"
            UPDATE dbo.Vehicles
            SET Make = @Make,
                Model = @Model,
                LicensePlate = @LicensePlate,
                ManufactureYear = @ManufactureYear
            WHERE Id = @Id;";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, vehicle);
        command.Parameters.AddWithValue("@Id", vehicle.Id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    // ---------- DELETE ----------
    /// <returns>true if a row was deleted.</returns>
    public bool Delete(int id)
    {
        const string sql = "DELETE FROM dbo.Vehicles WHERE Id = @Id;";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static void AddParameters(SqlCommand command, Vehicle v)
    {
        command.Parameters.AddWithValue("@Make", v.Make);
        command.Parameters.AddWithValue("@Model", v.Model);
        command.Parameters.AddWithValue("@LicensePlate", v.LicensePlate);
        command.Parameters.AddWithValue("@ManufactureYear", v.ManufactureYear);
    }

    private static Vehicle Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Make = r.GetString(1),
        Model = r.GetString(2),
        LicensePlate = r.GetString(3),
        ManufactureYear = r.GetInt32(4)
    };
}
