using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly ReturnEquipmentService _returnService;

    public ObservableCollection<Borrowing> Borrowings { get; } = new();

    [ObservableProperty]
    private Borrowing? _selectedBorrowing;

    [ObservableProperty]
    private string _message = string.Empty;

    // Lets the view color the message red on failure, neutral on info/success.
    [ObservableProperty]
    private bool _isError;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        ReturnEquipmentService returnService)
    {
        _borrowingRepository = borrowingRepository;
        _returnService = returnService;
    }

    public async Task LoadBorrowingsAsync()
    {
        var borrowings = await _borrowingRepository.GetAllAsync();

        Borrowings.Clear();

        foreach (var borrowing in borrowings)
        {
            if (borrowing.Status == BorrowingStatus.Active)
            {
                Borrowings.Add(borrowing);
            }
        }
    }

    [RelayCommand]
    private async Task ReturnSelectedBorrowingAsync()
    {
        if (SelectedBorrowing == null)
        {
            Message = "Please select a borrowing to return.";
            IsError = true;
            return;
        }

        try
        {
            await _returnService.ReturnAsync(SelectedBorrowing.Id);

            Message = $"Successfully returned borrowing #{SelectedBorrowing.Id}.";
            IsError = false;

            SelectedBorrowing = null;

            await LoadBorrowingsAsync();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            IsError = true;
        }
    }
}