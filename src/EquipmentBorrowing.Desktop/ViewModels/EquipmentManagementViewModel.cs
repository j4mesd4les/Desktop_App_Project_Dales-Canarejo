using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentManagementViewModel(EquipmentManagementService service) : ViewModelBase
{
    public ObservableCollection<Equipment> Equipment { get; } = new();

    [ObservableProperty] private Equipment? _selectedEquipment;
    [ObservableProperty] private string _equipmentNumber = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _type = "General";
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private bool _isLoading;

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        EquipmentNumber = value?.EquipmentNumber ?? string.Empty;
        Name = value?.Name ?? string.Empty;
        Type = value?.Type ?? "General";
        Description = value?.Description ?? string.Empty;
    }

    public async Task LoadAsync()
    {
        var selectedId = SelectedEquipment?.Id;
        IsLoading = true;
        try
        {
            var records = await service.GetAllAsync();
            Equipment.Clear();
            foreach (var item in records) Equipment.Add(item);
            if (selectedId.HasValue)
                SelectedEquipment = Equipment.FirstOrDefault(e => e.Id == selectedId.Value);
            Message = Equipment.Count == 0 ? "No equipment registered yet." : string.Empty;
            IsError = false;
        }
        catch (Exception)
        {
            Message = "Could not load equipment. Check the database connection and try again.";
            IsError = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void NewEquipment()
    {
        SelectedEquipment = null;
        EquipmentNumber = Name = Description = string.Empty;
        Type = "General";
        Message = "Enter the item details, then choose Register.";
        IsError = false;
    }

    [RelayCommand]
    private async Task SaveEquipmentAsync()
    {
        IsLoading = true;
        try
        {
            var isNew = SelectedEquipment is null;
            var selectedId = SelectedEquipment?.Id;
            var saved = isNew
                ? await service.RegisterAsync(EquipmentNumber, Name, Type, Description)
                : await service.UpdateAsync(selectedId!.Value, EquipmentNumber, Name, Type, Description);
            await LoadAsync();
            if (IsError)
            {
                Message = isNew
                    ? "Equipment was saved, but the list could not refresh. Reopen this page to confirm the item."
                    : "Equipment changes were saved, but the list could not refresh. Reopen this page to confirm them.";
                IsError = true;
                return;
            }
            SelectedEquipment = Equipment.FirstOrDefault(e => e.Id == saved.Id);
            Message = isNew ? "Equipment registered as available." : "Equipment information updated.";
            IsError = false;
        }
        catch (ArgumentException ex) { Message = ex.Message; IsError = true; }
        catch (DuplicateIdentifierError ex) { Message = ex.Message; IsError = true; }
        catch (Exception)
        {
            Message = "Could not save the equipment. Refresh the list before retrying in case the save completed.";
            IsError = true;
        }
        finally { IsLoading = false; }
    }
}
