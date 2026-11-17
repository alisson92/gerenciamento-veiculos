using Microsoft.Data.SqlClient;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.UI;

public class VehicleMenu
{
    private readonly VehicleRepository _repository;

    public VehicleMenu(VehicleRepository repository) => _repository = repository;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=====================================");
            Console.WriteLine("       VEHICLE MANAGEMENT SYSTEM      ");
            Console.WriteLine("=====================================");
            Console.WriteLine("1 - Register a vehicle   (Create)");
            Console.WriteLine("2 - List vehicles        (Read)");
            Console.WriteLine("3 - Edit a vehicle       (Update)");
            Console.WriteLine("4 - Delete a vehicle     (Delete)");
            Console.WriteLine("0 - Exit");
            Console.Write("\nChoose an option: ");

            var option = ConsoleHelper.ReadLine().Trim();
            Console.WriteLine();

            try
            {
                switch (option)
                {
                    case "1": CreateVehicle(); break;
                    case "2": ListVehicles(); break;
                    case "3": UpdateVehicle(); break;
                    case "4": DeleteVehicle(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                // 2601/2627 = unique index / constraint violation
                Console.WriteLine("Error: a vehicle with this license plate is already registered.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }

            ConsoleHelper.Pause();
        }
    }

    private void CreateVehicle()
    {
        Console.WriteLine("--- Register new vehicle ---");
        var vehicle = new Vehicle
        {
            Make = ConsoleHelper.ReadRequired("Make (e.g. Toyota)", 50),
            Model = ConsoleHelper.ReadRequired("Model (e.g. Corolla)", 50),
            LicensePlate = ConsoleHelper.ReadLicensePlate(),
            ManufactureYear = ConsoleHelper.ReadYear()
        };

        vehicle.Id = _repository.Create(vehicle);
        Console.WriteLine($"\nVehicle registered successfully with Id {vehicle.Id}.");
    }

    private void ListVehicles()
    {
        Console.WriteLine("--- Registered vehicles ---");
        var vehicles = _repository.GetAll();

        if (vehicles.Count == 0)
        {
            Console.WriteLine("No vehicles registered yet.");
            return;
        }

        Console.WriteLine($"{"Id",-5}{"Make",-18}{"Model",-20}{"Plate",-12}{"Year",-6}");
        Console.WriteLine(new string('-', 61));
        foreach (var v in vehicles)
            Console.WriteLine($"{v.Id,-5}{v.Make,-18}{v.Model,-20}{v.LicensePlate,-12}{v.ManufactureYear,-6}");
        Console.WriteLine($"\nTotal: {vehicles.Count} vehicle(s).");
    }

    private void UpdateVehicle()
    {
        Console.WriteLine("--- Edit vehicle ---");
        var id = ConsoleHelper.ReadId("Id of the vehicle to edit: ");
        if (id is null) { Console.WriteLine("Invalid Id."); return; }

        var vehicle = _repository.GetById(id.Value);
        if (vehicle is null) { Console.WriteLine("Vehicle not found."); return; }

        Console.WriteLine("Press ENTER to keep the current value shown in [brackets].\n");
        vehicle.Make = ConsoleHelper.ReadRequired("Make", 50, vehicle.Make);
        vehicle.Model = ConsoleHelper.ReadRequired("Model", 50, vehicle.Model);
        vehicle.LicensePlate = ConsoleHelper.ReadLicensePlate(vehicle.LicensePlate);
        vehicle.ManufactureYear = ConsoleHelper.ReadYear(vehicle.ManufactureYear);

        Console.WriteLine(_repository.Update(vehicle)
            ? "\nVehicle updated successfully."
            : "\nNo changes were made.");
    }

    private void DeleteVehicle()
    {
        Console.WriteLine("--- Delete vehicle ---");
        var id = ConsoleHelper.ReadId("Id of the vehicle to delete: ");
        if (id is null) { Console.WriteLine("Invalid Id."); return; }

        var vehicle = _repository.GetById(id.Value);
        if (vehicle is null) { Console.WriteLine("Vehicle not found."); return; }

        Console.WriteLine($"Selected: {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}, {vehicle.ManufactureYear})");
        if (!ConsoleHelper.Confirm("Are you sure you want to delete this vehicle?"))
        {
            Console.WriteLine("Operation cancelled.");
            return;
        }

        Console.WriteLine(_repository.Delete(id.Value)
            ? "\nVehicle deleted successfully."
            : "\nVehicle could not be deleted.");
    }
}
