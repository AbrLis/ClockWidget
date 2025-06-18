using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using System.Windows;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для главного окна виджета часов.
/// Управляет отображением времени, прозрачностью и другими настройками виджета.
/// </summary>
public partial class MainWindowViewModel : INotifyPropertyChanged, ISettingsViewModel, IDisposable
{
    private readonly TimeService _timeService;
    private readonly SettingsService _settingsService;
    private readonly ILogger<MainWindowViewModel> _logger = LoggingService.CreateLogger<MainWindowViewModel>();
    private bool _disposed;

    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindowViewModel"/>.
    /// Загружает сохраненные настройки и запускает сервис обновления времени.
    /// </summary>
    public MainWindowViewModel()
    {
        try
        {
            _logger.LogInformation("Initializing main window view model");
            _timeService = App.TimeService;
            _settingsService = App.SettingsService;
            var settings = _settingsService.CurrentSettings;
            _logger.LogInformation("Loading settings for main window: {Settings}", 
                JsonSerializer.Serialize(settings));
            // Инициализация значений и подписка на события вынесены в partial-файл
            InitializeFromSettings(settings);
            _timeService.TimeUpdated += OnTimeUpdated;
            OnTimeUpdated(this, DateTime.Now);
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateWindowsVisibility();
                _logger.LogInformation("Windows visibility updated after initialization");
            }));
            _logger.LogInformation("Main window view model initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing main window view model");
            throw;
        }
    }

    /// <summary>
    /// Вызывает событие <see cref="PropertyChanged"/>.
    /// </summary>
    /// <param name="propertyName">Имя изменившегося свойства.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Освобождает ресурсы, используемые экземпляром класса <see cref="MainWindowViewModel"/>.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _logger.LogInformation("Disposing main window view model");
                if (_timeService != null)
                    _timeService.TimeUpdated -= OnTimeUpdated;
                // Не вызываем _timeService.Dispose(), если это singleton из App
                _disposed = true;
                _logger.LogInformation("Main window view model disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing main window view model");
            }
        }
    }
} 