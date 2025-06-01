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
        BackgroundOpacitySlider.Value = 0.9;
        TextOpacitySlider.Value = 1.0;
        FontSizeSlider.Value = 48;
        UpdateAllValues();
        
        // Применяем начальные значения
        _mainWindow.SetBackgroundOpacity(BackgroundOpacitySlider.Value);
        _mainWindow.SetTextOpacity(TextOpacitySlider.Value);
        _mainWindow.SetFontSize(FontSizeSlider.Value);
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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _mainWindow.IsSettingsWindowOpen = false;
    }
} 