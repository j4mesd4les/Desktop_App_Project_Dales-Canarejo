using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly BorrowEquipmentService _borrowService;

    public ObservableCollection<Equipment> Equipment { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private string _message = string.Empty;

    // Inject the BorrowEquipmentService alongside the repository
    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        BorrowEquipmentService borrowService)
    {
        _equipmentRepository = equipmentRepository;
        _borrowService = borrowService;
    }

    public async Task LoadEquipmentAsync()
    {
        var equipment = await _equipmentRepository.GetAllAsync();
        Equipment.Clear();
        foreach (var item in equipment)
        {
            Equipment.Add(item);
        }
    }

    [RelayCommand]
    private async Task BorrowSelectedEquipmentAsync()
    {
        if (SelectedEquipment == null)
        {
            Message = "Please select an item to borrow.";
            return;
        }

        try
        {
            // Hardcoding Student ID 1 for now to match your manual tests
            await _borrowService.BorrowAsync(1, SelectedEquipment.Id);
            Message = $"Successfully borrowed: {SelectedEquipment.Name}";

            // Refresh the equipment list so the "Available: False" status updates visually
            await LoadEquipmentAsync();
        }
        catch (Exception ex)
        {
            // Catches Domain/Application errors (e.g., EquipmentNotAvailableError) and displays them to the user
            Message = ex.Message;
        }
    }
}