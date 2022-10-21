using QS.ViewModels;
using System;
using System.Windows.Input;

namespace Vodovoz.ViewModels.Commands
{
	public class OpenViewModelCommand : ICommand
	{
		private readonly Action<TabViewModelBase> _openViewModelAction;

		public OpenViewModelCommand(Action<TabViewModelBase> openViewModelAction)
		{
			_openViewModelAction = openViewModelAction ?? throw new ArgumentNullException(nameof(openViewModelAction));
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(TabViewModelBase viewModel)
		{
			return viewModel != null;
		}

		public bool CanExecute(object parameter)
		{
			return CanExecute(parameter as TabViewModelBase);
		}

		public void Execute(TabViewModelBase viewModel)
		{
			if(viewModel == null)
			{
				return;
			}
			_openViewModelAction?.Invoke(viewModel);
		}

		public void Execute(object parameter)
		{
			Execute(parameter as TabViewModelBase);
		}
	}
}
