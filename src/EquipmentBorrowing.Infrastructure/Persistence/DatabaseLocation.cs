namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// Reads the local PostgreSQL connection string from the environment so a
/// password is never stored in the repository.
/// </summary>
public static class DatabaseLocation
{
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("EQUIPMENTBORROWING_CONNECTION_STRING")
        ?? throw new InvalidOperationException(
            "Set EQUIPMENTBORROWING_CONNECTION_STRING to connect to PostgreSQL.");
}
