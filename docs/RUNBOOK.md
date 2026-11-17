# Runbook – Vehicle Management System (C# + SQL Server)

Console application that performs the four CRUD operations (Create, Read, Update, Delete) on vehicles
(make, model, license plate and year of manufacture), stored in SQL Server.

## 1. Project structure

```
VehicleManagement/
├── VehicleManagement.sln
├── database/
│   └── Vehicles.sql                  <- creates the database, table and sample data
├── docs/
│   └── RUNBOOK.md                    <- this file
└── src/VehicleManagement/
    ├── VehicleManagement.csproj
    ├── appsettings.json              <- connection string
    ├── Program.cs                    <- entry point / connection check
    ├── Models/Vehicle.cs             <- entity
    ├── Data/VehicleRepository.cs     <- CRUD with SQL (parameterized queries)
    └── UI/                           <- console menu and input validation
```

## 2. Prerequisites

| Item | Version | Notes |
|------|---------|-------|
| .NET SDK | 10.0 or newer | Installed with Visual Studio 2026, or https://dotnet.microsoft.com/download – check with `dotnet --version` |
| SQL Server | 2017 or newer | Express, Developer, LocalDB or a Docker container |
| SQL client | any | SSMS, Azure Data Studio, VS Code (mssql extension) or `sqlcmd` |
| Internet (first run only) | – | `dotnet restore` downloads the `Microsoft.Data.SqlClient` NuGet package |

## 3. Step by step

### Step 1 – Extract the project
Unzip the submitted `.zip` into any folder.

### Step 2 – Make sure SQL Server is running
Pick **one** of the options:

* **A) SQL Server installed on Windows (Express/Developer):** check that the *SQL Server (MSSQLSERVER)* or
  *SQL Server (SQLEXPRESS)* service is running (`services.msc`).
* **B) LocalDB (comes with Visual Studio):** nothing to start; use `(localdb)\MSSQLLocalDB` as the server.
* **C) Docker (Windows, macOS or Linux):**
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
             -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-CU15-ubuntu-22.04
  ```

### Step 3 – Create the database
Run `database/Vehicles.sql` in your SQL client. The script is idempotent (it can be run again safely) and creates:

* database `VehicleManagementDB`
* table `dbo.Vehicles` (`Id` identity PK, `Make`, `Model`, `LicensePlate` UNIQUE, `ManufactureYear` with a CHECK constraint)
* 3 sample vehicles (only if the table is empty)

Using `sqlcmd` (examples):
```bash
# SQL Express instance, Windows authentication (default setup)
sqlcmd -S .\SQLEXPRESS -E -i database/Vehicles.sql
# Default instance (MSSQLSERVER), Windows authentication
sqlcmd -S localhost -E -i database/Vehicles.sql
# Docker / SQL authentication
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -C -i database/Vehicles.sql
```
With SSMS / Azure Data Studio: open the file and press **Execute (F5)**.

### Step 4 – Configure the connection string
Edit `src/VehicleManagement/appsettings.json` and keep **only the line that matches your setup**:

```jsonc
// SQL Server Express, Windows auth (default value - works as is)
"VehicleDb": "Server=.\\SQLEXPRESS;Database=VehicleManagementDB;Integrated Security=True;TrustServerCertificate=True;"

// Default instance (MSSQLSERVER), Windows auth
"VehicleDb": "Server=localhost;Database=VehicleManagementDB;Integrated Security=True;TrustServerCertificate=True;"

// LocalDB
"VehicleDb": "Server=(localdb)\\MSSQLLocalDB;Database=VehicleManagementDB;Integrated Security=True;TrustServerCertificate=True;"

// Docker / SQL authentication
"VehicleDb": "Server=localhost,1433;Database=VehicleManagementDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
```
Alternatively, without editing the file, set the environment variable `ConnectionStrings__VehicleDb`
(e.g. PowerShell: `$env:ConnectionStrings__VehicleDb = "Server=...;..."`).

### Step 5 – Build and run
From the folder that contains `VehicleManagement.sln`:
```bash
dotnet restore
dotnet build
dotnet run --project src/VehicleManagement
```
(Or open `VehicleManagement.sln` in Visual Studio / Rider and press **F5**.)

If the connection works you will see the main menu:

```
=====================================
       VEHICLE MANAGEMENT SYSTEM
=====================================
1 - Register a vehicle   (Create)
2 - List vehicles        (Read)
3 - Edit a vehicle       (Update)
4 - Delete a vehicle     (Delete)
0 - Exit
```

## 4. Test plan (checks all CRUD operations)

Start with the sample data loaded by the script (3 vehicles).

| # | Operation | Action | Expected result |
|---|-----------|--------|-----------------|
| 1 | **Read** | Option `2` | Table with the 3 sample vehicles and `Total: 3 vehicle(s).` |
| 2 | **Create** | Option `1` → Make `Ford`, Model `Ka`, Plate `FOR1D99`, Year `2019` | `Vehicle registered successfully with Id 4.` |
| 3 | **Read** | Option `2` | The new vehicle appears (4 rows) |
| 4 | **Create (validation)** | Option `1` with the same plate `FOR1D99` | Error: plate already registered (no new row) |
| 5 | **Create (validation)** | Option `1` with Year `1800` or `abc` | Message asking for a valid year; prompt repeats |
| 6 | **Update** | Option `3` → Id `4` → ENTER on Make/Model/Plate, Year `2021` | `Vehicle updated successfully.` |
| 7 | **Read** | Option `2` | Id 4 now shows year 2021 |
| 8 | **Update (not found)** | Option `3` → Id `999` | `Vehicle not found.` |
| 9 | **Delete** | Option `4` → Id `4` → `y` | `Vehicle deleted successfully.` |
| 10 | **Read** | Option `2` | Back to 3 vehicles |
| 11 | **Delete (cancel)** | Option `4` → Id `1` → `n` | `Operation cancelled.`; vehicle 1 still listed |

You can also confirm the data directly in the database:
```sql
USE VehicleManagementDB;
SELECT * FROM dbo.Vehicles;
```

## 5. Troubleshooting

| Symptom | Likely cause / fix |
|---------|--------------------|
| `Could not connect to SQL Server / database` on start | SQL Server service not running, wrong server name in `appsettings.json`, or `Vehicles.sql` was not executed yet |
| `Cannot open database "VehicleManagementDB"` | Run `database/Vehicles.sql` first (Step 3) |
| `Login failed for user` | Wrong user/password, or SQL authentication not enabled (mixed mode) on the server |
| Certificate / SSL error | Keep `TrustServerCertificate=True` in the connection string (development only) |
| `dotnet: command not found` | Install the .NET 10 SDK (section 2) |
| `Error: a vehicle with this license plate is already registered` | Expected: `LicensePlate` has a UNIQUE constraint |

## 6. Design notes

* **ADO.NET with `Microsoft.Data.SqlClient`** – plain SQL, same approach as a classic class example, no ORM.
* **Parameterized queries** everywhere, preventing SQL injection.
* **Layers:** `Models` (data), `Data` (SQL access), `UI` (console), keeping the SQL out of the menu code.
* **Validation** at two levels: in the console (required fields, lengths, year range) and in the database
  (`NOT NULL`, `UNIQUE` plate, `CHECK` on the year).
