using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Desktop.Views;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public EquipmentViewModel EquipmentView { get; }

    public BorrowingsViewModel BorrowingsView { get; }

    public StudentManagementViewModel StudentsView { get; }

    public EquipmentManagementViewModel EquipmentManagementView { get; }

    [ObservableProperty]
    private ViewModelBase _currentView;

    // Drives which sidebar button gets the "active" highlighted style.
    public bool IsEquipmentActive => CurrentView == EquipmentView;

    public bool IsBorrowingsActive => CurrentView == BorrowingsView;

    public bool IsStudentsActive => CurrentView == StudentsView;

    public bool IsEquipmentManagementActive => CurrentView == EquipmentManagementView;

    public MainViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel,
        StudentManagementViewModel studentsViewModel,
        EquipmentManagementViewModel equipmentManagementViewModel)
    {
        EquipmentView = equipmentViewModel;
        BorrowingsView = borrowingsViewModel;
        StudentsView = studentsViewModel;
        EquipmentManagementView = equipmentManagementViewModel;

        _currentView = EquipmentView;
    }

    // Refresh navigation button states whenever the current view changes.
    partial void OnCurrentViewChanged(ViewModelBase value)
    {
        OnPropertyChanged(nameof(IsEquipmentActive));
        OnPropertyChanged(nameof(IsBorrowingsActive));
        OnPropertyChanged(nameof(IsStudentsActive));
        OnPropertyChanged(nameof(IsEquipmentManagementActive));
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentView = EquipmentView;
    }

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        // Refresh the borrowing list every time the user opens
        // the Active Borrowings page.
        await BorrowingsView.LoadBorrowingsAsync();

        CurrentView = BorrowingsView;
    }

    [RelayCommand]
    private async Task ShowStudentsAsync()
    {
        CurrentView = StudentsView;
        await StudentsView.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowEquipmentManagementAsync()
    {
        CurrentView = EquipmentManagementView;
        await EquipmentManagementView.LoadAsync();
    }
}

