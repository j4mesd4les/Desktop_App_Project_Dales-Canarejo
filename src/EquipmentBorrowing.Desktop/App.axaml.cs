using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

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

            // Repositories
            services.AddSingleton<IStudentRepository>(
                new InMemoryStudentRepository(
                    new Dictionary<int, Student>
                    {
                        { 1, new Student { Id = 1, Name = "Dodong" } }
                    }));

            services.AddSingleton<IEquipmentRepository>(
                new InMemoryEquipmentRepository(
                    new Dictionary<int, Equipment>
                    {
                        { 1, new Equipment { Id = 1, Name = "HDMI Projector" } },
                        { 2, new Equipment { Id = 2, Name = "Laptop" } },
                        { 3, new Equipment { Id = 3, Name = "Extension Cord" } },
                        { 4, new Equipment { Id = 4, Name = "Speaker" } }
                    }));

            services.AddSingleton<IBorrowingRepository>(
                new InMemoryBorrowingRepository());

            // Application services
            services.AddTransient<BorrowEquipmentService>();
            services.AddTransient<ReturnEquipmentService>();

            // ViewModels
            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<MainViewModel>();

            var serviceProvider = services.BuildServiceProvider();

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

