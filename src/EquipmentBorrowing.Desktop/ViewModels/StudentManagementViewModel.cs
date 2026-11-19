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

public partial class StudentManagementViewModel(StudentManagementService service) : ViewModelBase
{
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private string _studentNumber = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _contact = string.Empty;
    [ObservableProperty] private bool _eligibilityAllowed = true;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private bool _isLoading;

    partial void OnSelectedStudentChanged(Student? value)
    {
        StudentNumber = value?.StudentNumber ?? string.Empty;
        Name = value?.Name ?? string.Empty;
        Contact = value?.Contact ?? string.Empty;
        EligibilityAllowed = value?.IsAllowedToBorrow ?? true;
    }

    public async Task LoadAsync()
    {
        var selectedId = SelectedStudent?.Id;
        IsLoading = true;
        try
        {
            var records = await service.GetAllAsync();
            Students.Clear();
            foreach (var student in records) Students.Add(student);
            if (selectedId.HasValue)
                SelectedStudent = Students.FirstOrDefault(s => s.Id == selectedId.Value);
            Message = Students.Count == 0 ? "No students registered yet." : string.Empty;
            IsError = false;
        }
        catch (Exception)
        {
            Message = "Could not load students. Check the database connection and try again.";
            IsError = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void NewStudent()
    {
        SelectedStudent = null;
        StudentNumber = Name = Contact = string.Empty;
        EligibilityAllowed = true;
        Message = "Enter the student details, then choose Register.";
        IsError = false;
    }

    [RelayCommand]
    private async Task SaveStudentAsync()
    {
        IsLoading = true;
        try
        {
            var isNew = SelectedStudent is null;
            var selectedId = SelectedStudent?.Id;
            var saved = isNew
                ? await service.RegisterAsync(StudentNumber, Name, Contact)
                : await service.UpdateAsync(selectedId!.Value, StudentNumber, Name, Contact);
            await LoadAsync();
            if (IsError)
            {
                Message = isNew
                    ? "Student was saved, but the list could not refresh. Reopen this page to confirm the record."
                    : "Student changes were saved, but the list could not refresh. Reopen this page to confirm them.";
                IsError = true;
                return;
            }
            SelectedStudent = Students.FirstOrDefault(s => s.Id == saved.Id);
            Message = isNew ? "Student registered." : "Student information updated.";
            IsError = false;
        }
        catch (ArgumentException ex) { Message = ex.Message; IsError = true; }
        catch (DuplicateIdentifierError ex) { Message = ex.Message; IsError = true; }
        catch (Exception)
        {
            Message = "Could not save the student. Refresh the list before retrying in case the save completed.";
            IsError = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SaveEligibilityAsync()
    {
        if (SelectedStudent is null)
        {
            Message = "Select a student first.";
            IsError = true;
            return;
        }

        if (SelectedStudent.IsAllowedToBorrow == EligibilityAllowed)
        {
            Message = "The student's eligibility is already set that way.";
            IsError = false;
            return;
        }

        IsLoading = true;
        try
        {
            var saved = await service.SetEligibilityAsync(SelectedStudent.Id, EligibilityAllowed);
            Message = saved.IsAllowedToBorrow ? "Student may borrow equipment." : "Student is restricted from new borrowings.";
            IsError = false;
            await LoadAsync();
        }
        catch (Exception)
        {
            Message = "Could not update eligibility. Refresh the list before retrying.";
            IsError = true;
        }
        finally { IsLoading = false; }
    }
}
