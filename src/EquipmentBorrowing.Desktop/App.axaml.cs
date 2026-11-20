using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            services.AddPersistence();

            services.AddTransient<BorrowEquipmentService>();
            services.AddTransient<ReturnEquipmentService>();
            services.AddTransient<StudentManagementService>();
            services.AddTransient<EquipmentManagementService>();

            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<StudentManagementViewModel>();
            services.AddTransient<EquipmentManagementViewModel>();
            services.AddTransient<MainViewModel>();

            var serviceProvider = services.BuildServiceProvider();
            var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

            // Create the window first so it always appears,
            // then load the database in the background.
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            _ = StartDatabaseAsync(serviceProvider, mainViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task StartDatabaseAsync(
        IServiceProvider serviceProvider,
        MainViewModel mainViewModel)
    {
        try
        {
            await serviceProvider.InitializeDatabaseAsync();
            await mainViewModel.EquipmentView.LoadEquipmentAsync();
            await mainViewModel.EquipmentView.LoadStudentsAsync();
            await mainViewModel.BorrowingsView.LoadBorrowingsAsync();
        }
        catch (Exception)
        {
            mainViewModel.EquipmentView.Message =
                "Could not connect to the database. Check that PostgreSQL is running and the connection string is set, then restart the app.";
            mainViewModel.EquipmentView.IsError = true;
        }
    }
}