using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly BorrowEquipmentService _borrowService;

public ObservableCollection<Equipment> Equipment { get; } = new();

    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private DateTimeOffset? _selectedExpectedReturnDate;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isError;

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        BorrowEquipmentService borrowService,
        IStudentRepository studentRepository)
    {
        _equipmentRepository = equipmentRepository;
        _borrowService = borrowService;
        _studentRepository = studentRepository;
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

    public async Task LoadStudentsAsync()
    {
        Students.Clear();

        // Currently the application has one student in the
        // in-memory repository.
        var student = await _studentRepository.GetByIdAsync(1);

        if (student != null)
        {
            Students.Add(student);
        }
    }

    [RelayCommand]
    private async Task BorrowSelectedEquipmentAsync()
    {
        if (SelectedStudent == null)
        {
            Message = "Please select a student.";
            IsError = true;
            return;
        }

        if (SelectedEquipment == null)
        {
            Message = "Please select an item to borrow.";
            IsError = true;
            return;
        }

        if (SelectedExpectedReturnDate == null)
        {
            Message = "Please select an expected return date.";
            IsError = true;
            return;
        }

        try
        {
            await _borrowService.BorrowAsync(
                SelectedStudent.Id,
                SelectedEquipment.Id,
                SelectedExpectedReturnDate.Value.DateTime);

            Message = $"Successfully borrowed: {SelectedEquipment.Name}";
            IsError = false;

            await LoadEquipmentAsync();

            SelectedEquipment = null;
            SelectedExpectedReturnDate = null;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            IsError = true;
        }
    }
}
