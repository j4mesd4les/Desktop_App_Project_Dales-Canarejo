namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// The one place that decides where the SQLite file lives, so the running
/// app and the "dotnet ef" tools always use the same database.
///
/// The file goes in the user's local application data folder
/// (on Windows: C:\Users\&lt;you&gt;\AppData\Local\EquipmentBorrowing\equipment-borrowing.db).
/// A fixed location means the data survives rebuilds and does not depend on
/// which folder the app happened to be started from.
/// </summary>
public static class DatabaseLocation
{
    public static string DatabasePath { get; } = BuildPath();

    public static string ConnectionString => $"Data Source={DatabasePath}";

    private static string BuildPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EquipmentBorrowing");

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, "equipment-borrowing.db");
    }
}
