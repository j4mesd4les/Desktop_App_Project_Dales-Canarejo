using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
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
            var students = new InMemoryStudentRepository(new Dictionary<int, Student>
            {
                { 1, new Student { Id = 1, Name = "Dodong" } }
            });

            var equipmentRepository = new InMemoryEquipmentRepository(new Dictionary<int, Equipment>
            {
                { 1, new Equipment { Id = 1, Name = "HDMI Projector" } },
                { 2, new Equipment { Id = 2, Name = "Laptop" } },
                { 3, new Equipment { Id = 3, Name = "Extension Cord" } },
                { 4, new Equipment { Id = 4, Name = "Speaker" } }
            });

            var borrowings = new InMemoryBorrowingRepository();

            // Wire up the application service
            var borrowService = new BorrowEquipmentService(students, equipmentRepository, borrowings);

            // Pass dependencies to the ViewModel
            var equipmentViewModel = new EquipmentViewModel(equipmentRepository, borrowService);
            await equipmentViewModel.LoadEquipmentAsync();

            var mainViewModel = new MainViewModel(equipmentViewModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}