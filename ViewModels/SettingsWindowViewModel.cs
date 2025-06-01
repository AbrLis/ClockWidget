using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClockWidgetApp.ViewModels;

public class SettingsWindowViewModel : INotifyPropertyChanged
{
    private readonly MainWindowViewModel _mainViewModel;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double BackgroundOpacity
    {
        get => _mainViewModel.BackgroundOpacity;
        set => _mainViewModel.BackgroundOpacity = value;
    }

    public double TextOpacity
    {
        get => _mainViewModel.TextOpacity;
        set => _mainViewModel.TextOpacity = value;
    }

    public double FontSize
    {
        get => _mainViewModel.FontSize;
        set => _mainViewModel.FontSize = value;
    }

    public bool ShowSeconds
    {
        get => _mainViewModel.ShowSeconds;
        set => _mainViewModel.ShowSeconds = value;
    }

    public SettingsWindowViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 