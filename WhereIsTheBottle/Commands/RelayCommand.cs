using System;
using System.Windows.Input;

namespace WhereIsTheBottle.Commands
{
	public class RelayCommand<T> : ICommand
	{
		private readonly Func<T, bool> canExecute;
		private readonly Action<T> execute;

		public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
		{
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public bool CanExecute(object parameter)
		{
			return canExecute?.Invoke((T)parameter) ?? true;
		}

		public void Execute(object parameter)
		{
			execute((T)parameter);
		}
	}

	public class RelayCommand : ICommand
	{
		private readonly Func<bool> canExecute;
		private readonly Action execute;

		public RelayCommand(Action execute, Func<bool> canExecute = null)
		{
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public bool CanExecute(object parameter)
		{
			return canExecute?.Invoke() ?? true;
		}

		public void Execute(object parameter)
		{
			execute();
		}
	}
}