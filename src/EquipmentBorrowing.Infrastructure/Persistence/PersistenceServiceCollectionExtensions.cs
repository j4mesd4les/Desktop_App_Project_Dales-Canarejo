using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// The single place where the database is wired into the app. The Desktop
/// project calls AddPersistence() and InitializeDatabaseAsync() and never
/// mentions SQLite, EF Core or the DbContext itself.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // A factory (instead of one shared DbContext) lets every repository
        // method create a short-lived context, which suits a desktop app whose
        // ViewModels live for the whole session.
        services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
        {
            options.UseSqlite(DatabaseLocation.ConnectionString);

#if DEBUG
            // While debugging, print every SQL command EF Core runs to the
            // Visual Studio Output window (Debug). Used to inspect the SQL
            // generated for the LINQ queries (Lab 3, Part N).
            options.LogTo(
                message => System.Diagnostics.Debug.WriteLine(message),
                new[] { RelationalEventId.CommandExecuted });
#endif
        });

        // The repositories hold no state of their own (the factory is
        // registered as a singleton), so one instance of each is enough.
        services.AddSingleton<IStudentRepository, EfStudentRepository>();
        services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
        services.AddSingleton<IBorrowingRepository, EfBorrowingRepository>();

        return services;
    }

    /// <summary>
    /// Brings the database up to date by applying any migrations that have not
    /// been applied yet. It creates the file if it is missing, does nothing if
    /// it is already current, and never deletes or overwrites existing data.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var contextFactory = services
            .GetRequiredService<IDbContextFactory<EquipmentBorrowingDbContext>>();

        await using var context =
            await contextFactory.CreateDbContextAsync(cancellationToken);

        await context.Database.MigrateAsync(cancellationToken);
    }
}
