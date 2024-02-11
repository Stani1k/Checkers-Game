using System.Windows.Input;
using Checkers.Tools;
using Checkers.WPF.Commands;

namespace Checkers.WPF;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _selectedViewModel;
    
    public ICommand UpdateViewCommand { get; set; }
    
    public MainViewModel()
    {
        UpdateViewCommand = new UpdateViewCommand(this);
    }
    
    public ViewModelBase? SelectedViewModel
    {
        get => _selectedViewModel;
        set
        {
            if (Equals(value, _selectedViewModel)) return;
            _selectedViewModel = value;
            OnPropertyChanged();
        }
    }
}