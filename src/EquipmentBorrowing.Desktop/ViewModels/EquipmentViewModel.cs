using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;

    public ObservableCollection<Equipment> Equipment { get; } = new();

    public EquipmentViewModel(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
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
}