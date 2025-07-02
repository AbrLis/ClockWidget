using System.Windows.Input;

/// <summary>
/// Простая реализация ICommand для биндинга команд.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;
    /// <summary>
    /// Конструктор RelayCommand.
    /// </summary>
    /// <param name="execute">Действие для выполнения.</param>
    /// <param name="canExecute">Условие доступности команды.</param>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
} 