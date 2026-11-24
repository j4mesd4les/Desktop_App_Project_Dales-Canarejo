using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Client;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
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

            var apiBaseUrl = Environment.GetEnvironmentVariable("EQUIPMENTBORROWING_API_BASE_URL")
                ?? "http://localhost:5080/";
            var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/") };
            services.AddSingleton<IEquipmentBorrowingApiClient>(new EquipmentBorrowingApiClient(httpClient));

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

            _ = LoadApiDataAsync(mainViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task LoadApiDataAsync(MainViewModel mainViewModel)
    {
        try
        {
            await mainViewModel.EquipmentView.LoadEquipmentAsync();
            await mainViewModel.EquipmentView.LoadStudentsAsync();
            await mainViewModel.BorrowingsView.LoadBorrowingsAsync();
        }
        catch (Exception)
        {
            mainViewModel.EquipmentView.Message =
                "Could not connect to the API. Start the API and check its URL, then refresh this page.";
            mainViewModel.EquipmentView.IsError = true;
        }
    }
}
