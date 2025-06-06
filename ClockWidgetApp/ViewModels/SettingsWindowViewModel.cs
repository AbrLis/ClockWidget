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
        set
        {
            _mainViewModel.BackgroundOpacity = value;
            OnPropertyChanged();
        }
    }

    public double TextOpacity
    {
        get => _mainViewModel.TextOpacity;
        set
        {
            _mainViewModel.TextOpacity = value;
            OnPropertyChanged();
        }
    }

    public double FontSize
    {
        get => _mainViewModel.FontSize;
        set
        {
            _mainViewModel.FontSize = value;
            OnPropertyChanged();
        }
    }

    public bool ShowSeconds
    {
        get => _mainViewModel.ShowSeconds;
        set
        {
            _mainViewModel.ShowSeconds = value;
            OnPropertyChanged();
        }
    }

    public SettingsWindowViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        // Подписываемся на изменения свойств MainWindowViewModel
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Пробрасываем уведомления об изменениях свойств
        OnPropertyChanged(e.PropertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 