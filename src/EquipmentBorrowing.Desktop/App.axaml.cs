using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            // Repositories: now backed by SQLite through EF Core instead of
            // in-memory dictionaries. Registered in one place (Infrastructure).
            services.AddPersistence();

            // Application services
            services.AddTransient<BorrowEquipmentService>();
            services.AddTransient<ReturnEquipmentService>();

            // ViewModels
            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<MainViewModel>();

            var serviceProvider = services.BuildServiceProvider();

            // Creates the database on first run and applies any new
            // migrations. Existing data is never wiped.
            await serviceProvider.InitializeDatabaseAsync();

            var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

            await mainViewModel.EquipmentView.LoadEquipmentAsync();
            await mainViewModel.EquipmentView.LoadStudentsAsync();
            await mainViewModel.BorrowingsView.LoadBorrowingsAsync();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
