namespace VehicleManagement.UI;

/// <summary>Small helpers for reading validated input from the console.</summary>
public static class ConsoleHelper
{
    /// <summary>
    /// Reads one line from the console. Console.ReadLine() returns null when the input ends
    /// (Ctrl+Z on Windows, Ctrl+D on Linux/macOS, or redirected input that is over); without
    /// this check the input loops below would repeat forever, so the app exits gracefully.
    /// </summary>
    public static string ReadLine()
    {
        var line = Console.ReadLine();
        if (line is null)
        {
            Console.WriteLine("\nInput closed. Exiting.");
            Environment.Exit(0);
        }
        return line;
    }

    public static string ReadRequired(string label, int maxLength, string? current = null)
    {
        while (true)
        {
            var hint = current is null ? "" : $" [{current}]";
            Console.Write($"{label}{hint}: ");
            var input = ReadLine().Trim();

            if (string.IsNullOrEmpty(input) && current is not null)
                return current;                       // keep the existing value on update

            if (string.IsNullOrEmpty(input))
                Console.WriteLine("  ! This field is required.");
            else if (input.Length > maxLength)
                Console.WriteLine($"  ! Maximum {maxLength} characters.");
            else
                return input;
        }
    }

    public static string ReadLicensePlate(string? current = null)
    {
        while (true)
        {
            var value = ReadRequired("License plate (e.g. ABC1D23 or ABC-1234)", 10, current)
                .ToUpperInvariant();
            if (value.All(c => char.IsLetterOrDigit(c) || c == '-'))
                return value;
            Console.WriteLine("  ! Use only letters, digits and a hyphen.");
        }
    }

    public static int ReadYear(int? current = null)
    {
        var max = DateTime.Now.Year + 1;
        while (true)
        {
            var hint = current is null ? "" : $" [{current}]";
            Console.Write($"Year of manufacture (1900-{max}){hint}: ");
            var input = ReadLine().Trim();

            if (string.IsNullOrEmpty(input) && current is not null)
                return current.Value;

            if (int.TryParse(input, out var year) && year >= 1900 && year <= max)
                return year;
            Console.WriteLine($"  ! Enter a valid year between 1900 and {max}.");
        }
    }

    public static int? ReadId(string prompt)
    {
        Console.Write(prompt);
        return int.TryParse(ReadLine(), out var id) ? id : null;
    }

    public static bool Confirm(string question)
    {
        Console.Write($"{question} (y/n): ");
        return ReadLine().Trim().ToLowerInvariant() is "y" or "yes";
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press ENTER to continue...");
        ReadLine();
    }
}
