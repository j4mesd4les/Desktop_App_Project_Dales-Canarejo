using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Models;
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

    public ObservableCollection<ActiveBorrowingDetails> Borrowings { get; } = new();

    [ObservableProperty]
    private ActiveBorrowingDetails? _selectedBorrowing;

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
        // The database finds the active borrowings and joins in the student
        // and equipment names; the ViewModel just displays the result.
        var borrowings = await _borrowingRepository.GetActiveWithDetailsAsync();

        Borrowings.Clear();

        foreach (var borrowing in borrowings)
        {
            Borrowings.Add(borrowing);
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
            await _returnService.ReturnAsync(SelectedBorrowing.BorrowingId);

            Message = $"Successfully returned {SelectedBorrowing.EquipmentName} (borrowing #{SelectedBorrowing.BorrowingId}).";
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