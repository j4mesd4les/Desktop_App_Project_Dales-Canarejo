using CommunityToolkit.Mvvm.ComponentModel;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public EquipmentViewModel EquipmentView { get; }

    public MainViewModel(EquipmentViewModel equipmentViewModel)
    {
        EquipmentView = equipmentViewModel;
    }
}