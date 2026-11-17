namespace VehicleManagement.Models;

/// <summary>Represents one row of the dbo.Vehicles table.</summary>
public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int ManufactureYear { get; set; }
}
