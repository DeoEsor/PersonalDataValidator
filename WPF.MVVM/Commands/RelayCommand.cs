using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
namespace WPF.MVVM.Commands
{
	public sealed class RelayCommand : ICommand
	{
		private readonly Action<object?> _execute;
		private readonly Predicate<object?>? _canExecute;
		
		public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

		public void Execute(object? parameter) => _execute(parameter);
		
		public event EventHandler? CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
