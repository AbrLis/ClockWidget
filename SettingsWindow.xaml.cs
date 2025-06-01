using System;
using System.Windows;
using System.Windows.Controls;

namespace ClockWidgetApp;

public partial class SettingsWindow : Window
{
    private readonly MainWindow _mainWindow;

    public SettingsWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        
        // Устанавливаем начальные значения слайдеров
        BackgroundOpacitySlider.Value = _mainWindow._settings.BackgroundOpacity;
        TextOpacitySlider.Value = _mainWindow._settings.TextOpacity;
        FontSizeSlider.Value = _mainWindow._settings.FontSize;
        ShowSecondsCheckBox.IsChecked = _mainWindow._settings.ShowSeconds;
        UpdateAllValues();
        
        // Добавляем обработчик закрытия окна
        Closing += SettingsWindow_Closing;
    }

    private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Сохраняем текущие настройки
        _mainWindow._settings.Save();
        _mainWindow.IsSettingsWindowOpen = false;
    }

    private void BackgroundOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_mainWindow != null)
        {
            _mainWindow.SetBackgroundOpacity(BackgroundOpacitySlider.Value);
            UpdateAllValues();
        }
    }

    private void TextOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_mainWindow != null)
        {
            _mainWindow.SetTextOpacity(TextOpacitySlider.Value);
            UpdateAllValues();
        }
    }

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_mainWindow != null)
        {
            _mainWindow.SetFontSize(FontSizeSlider.Value);
            UpdateAllValues();
        }
    }

    private void ShowSecondsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_mainWindow != null)
        {
            _mainWindow.SetShowSeconds(ShowSecondsCheckBox.IsChecked ?? true);
        }
    }

    private void UpdateAllValues()
    {
        BackgroundOpacityValueText.Text = $"{Math.Round(BackgroundOpacitySlider.Value * 100)}%";
        TextOpacityValueText.Text = $"{Math.Round(TextOpacitySlider.Value * 100)}%";
        FontSizeValueText.Text = $"{Math.Round(FontSizeSlider.Value)}px";
    }

    private void CloseWidgetButton_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.Close();
        Close();
    }
} 