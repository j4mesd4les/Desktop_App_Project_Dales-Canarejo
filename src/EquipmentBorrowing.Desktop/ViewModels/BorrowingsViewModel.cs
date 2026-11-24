using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Client;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IEquipmentBorrowingApiClient _api;

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
        IEquipmentBorrowingApiClient api)
    {
        _api = api;
    }

    public async Task LoadBorrowingsAsync()
    {
        // The database finds the active borrowings and joins in the student
        // and equipment names; the ViewModel just displays the result.
        IsLoading = true;
        try
        {
            var students = await _api.GetStudentsAsync();
            var equipment = await _api.GetEquipmentAsync();
            var borrowings = await _api.GetBorrowingsAsync(status: "Active");
            Students.Clear(); foreach (var student in students.Select(x => x.ToModel())) Students.Add(student);
            EquipmentItems.Clear(); foreach (var item in equipment.Select(x => x.ToModel())) EquipmentItems.Add(item);
            Borrowings.Clear(); foreach (var row in borrowings.Select(x => x.ToActive())) Borrowings.Add(row);
            await SearchAsync();
        }
        catch (Exception ex) { Message = EquipmentBorrowingApiFeedback.For(ex); IsError = true; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsLoading = true;
            var sort = SortChoice.StartsWith("Checkout", StringComparison.OrdinalIgnoreCase) ? "borrowedDate" : "expectedReturnDate";
            var rows = await _api.GetBorrowingsAsync(SearchText, StatusChoice, sort, Descending ? "desc" : "asc");
            History.Clear(); foreach (var row in rows.Select(x => x.ToHistory())) History.Add(row);
            Message = rows.Count == 0 ? "No borrowing records match this search." : $"Showing {rows.Count} borrowing record(s).";
            IsError = false;
        }
        catch (Exception ex) { Message = EquipmentBorrowingApiFeedback.For(ex); IsError = true; }
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
            IReadOnlyList<BorrowingDto> rows;
            if (SelectedHistoryStudent is not null && SelectedHistoryEquipment is not null)
            {
                var studentRows = await _api.GetStudentHistoryAsync(SelectedHistoryStudent.Id);
                rows = studentRows.Where(r => r.EquipmentId == SelectedHistoryEquipment.Id).ToArray();
            }
            else if (SelectedHistoryStudent is not null) rows = await _api.GetStudentHistoryAsync(SelectedHistoryStudent.Id);
            else if (SelectedHistoryEquipment is not null) rows = await _api.GetEquipmentHistoryAsync(SelectedHistoryEquipment.Id);
            else rows = await _api.GetBorrowingsAsync();
            History.Clear(); foreach (var row in rows.Select(x => x.ToHistory())) History.Add(row);
            Message = rows.Count == 0 ? "No history was found for these filters." : $"Showing {rows.Count} history record(s).";
            IsError = false;
        }
        catch (Exception ex) { Message = EquipmentBorrowingApiFeedback.For(ex); IsError = true; }
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
            await _api.ReturnAsync(SelectedBorrowing.BorrowingId);

            Message = $"Successfully returned {SelectedBorrowing.EquipmentName} (borrowing #{SelectedBorrowing.BorrowingId}).";
            IsError = false;

            SelectedBorrowing = null;

            await LoadBorrowingsAsync();
        }
        catch (Exception ex)
        {
            var feedback = EquipmentBorrowingApiFeedback.For(ex);
            if (EquipmentBorrowingApiFeedback.NeedsRefresh(ex)) await LoadBorrowingsAsync();
            Message = feedback;
            IsError = true;
        }
    }
}
