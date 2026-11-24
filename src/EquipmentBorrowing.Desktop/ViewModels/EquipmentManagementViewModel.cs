using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Client;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentManagementViewModel(IEquipmentBorrowingApiClient api) : ViewModelBase
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
            var records = await api.GetEquipmentAsync();
            Equipment.Clear();
            foreach (var item in records) Equipment.Add(item.ToModel());
            if (selectedId.HasValue)
                SelectedEquipment = Equipment.FirstOrDefault(e => e.Id == selectedId.Value);
            Message = Equipment.Count == 0 ? "No equipment registered yet." : string.Empty;
            IsError = false;
        }
        catch (Exception ex)
        {
            Message = EquipmentBorrowingApiFeedback.For(ex);
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
                ? (await api.CreateEquipmentAsync(new CreateEquipmentRequest(EquipmentNumber, Name, Description, Type))).ToModel()
                : (await api.UpdateEquipmentAsync(selectedId!.Value, new UpdateEquipmentRequest(EquipmentNumber, Name, Description, Type))).ToModel();
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
        catch (Exception ex)
        {
            if (EquipmentBorrowingApiFeedback.NeedsRefresh(ex)) await LoadAsync();
            Message = EquipmentBorrowingApiFeedback.For(ex);
            IsError = true;
        }
        finally { IsLoading = false; }
    }
}
