using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly ReturnEquipmentService _returnService;
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ObservableCollection<ActiveBorrowingDetails> Borrowings { get; } = new();
    public ObservableCollection<BorrowingHistoryRecord> History { get; } = new();
    public IReadOnlyList<string> StatusOptions { get; } = new[] { "All", "Active", "Returned" };
    public IReadOnlyList<string> SortOptions { get; } = new[] { "Expected return date", "Checkout date" };
    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Equipment> EquipmentItems { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusChoice = "All";
    [ObservableProperty] private string _sortChoice = "Expected return date";
    [ObservableProperty] private bool _descending = true;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private Student? _selectedHistoryStudent;
    [ObservableProperty] private Equipment? _selectedHistoryEquipment;

    [ObservableProperty]
    private ActiveBorrowingDetails? _selectedBorrowing;

    [ObservableProperty]
    private string _message = string.Empty;

    // Lets the view color the message red on failure, neutral on info/success.
    [ObservableProperty]
    private bool _isError;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        ReturnEquipmentService returnService,
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _returnService = returnService;
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task LoadBorrowingsAsync()
    {
        // The database finds the active borrowings and joins in the student
        // and equipment names; the ViewModel just displays the result.
        IsLoading = true;
        var students = await _studentRepository.GetAllAsync();
        var equipment = await _equipmentRepository.GetAllAsync();
        Students.Clear(); foreach (var student in students) Students.Add(student);
        EquipmentItems.Clear(); foreach (var item in equipment) EquipmentItems.Add(item);
        var borrowings = await _borrowingRepository.GetActiveWithDetailsAsync();

        Borrowings.Clear();

        foreach (var borrowing in borrowings)
        {
            Borrowings.Add(borrowing);
        }
        await SearchAsync();
        IsLoading = false;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsLoading = true;
            var status = Enum.Parse<BorrowingStatusFilter>(StatusChoice, true);
            var sort = SortChoice.StartsWith("Checkout", StringComparison.OrdinalIgnoreCase)
                ? BorrowingSortField.CheckoutDate : BorrowingSortField.ExpectedReturnDate;
            var rows = await _borrowingRepository.SearchAsync(SearchText, status, sort, Descending);
            History.Clear(); foreach (var row in rows) History.Add(row);
            Message = rows.Count == 0 ? "No borrowing records match this search." : $"Showing {rows.Count} borrowing record(s).";
            IsError = false;
        }
        catch (Exception) { Message = "Could not load borrowing records. Check the database connection and try again."; IsError = true; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        StatusChoice = "All";
        SortChoice = "Expected return date";
        Descending = true;
        SelectedHistoryStudent = null;
        SelectedHistoryEquipment = null;
        await SearchAsync();
    }

    [RelayCommand]
    private async Task LoadSelectedHistoryAsync()
    {
        try
        {
            IsLoading = true;
            var rows = await _borrowingRepository.GetHistoryAsync(SelectedHistoryStudent?.Id, SelectedHistoryEquipment?.Id);
            History.Clear(); foreach (var row in rows) History.Add(row);
            Message = rows.Count == 0 ? "No history was found for these filters." : $"Showing {rows.Count} history record(s).";
            IsError = false;
        }
        catch (Exception) { Message = "Could not load history. Check the database connection and try again."; IsError = true; }
        finally { IsLoading = false; }
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
