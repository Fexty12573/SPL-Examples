using System;
using System.Windows.Input;

namespace FsmEditor;

public interface INodifyCommand : ICommand
{
    void RaiseCanExecuteChanged();
}

public class DelegateCommand(Action action, Func<bool>? executeCondition = default) : INodifyCommand
{
    private readonly Action _action = action ?? throw new ArgumentNullException(nameof(action));

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object parameter)
        => executeCondition?.Invoke() ?? true;

    public void Execute(object parameter)
        => _action();

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class DelegateCommand<T>(Action<T> action, Func<T, bool>? executeCondition = default)
    : INodifyCommand
{
    private readonly Action<T> _action = action ?? throw new ArgumentNullException(nameof(action));

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (parameter is T value)
        {
            return executeCondition?.Invoke(value) ?? true;
        }

        return executeCondition?.Invoke(default!) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T value)
        {
            _action(value);
        }
        else
        {
            _action(default!);
        }
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
