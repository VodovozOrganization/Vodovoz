using System;
namespace Vodovoz.ViewModelBased
{
	public class DelegateCommand : PropertySubscribedCommandBase
	{
		private readonly Func<bool> canExecute;
		private readonly Action execute;

		public DelegateCommand(Action execute, Func<bool> canExecute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public DelegateCommand(Action execute) : this(execute, null)
		{
		}

		public bool CanExecute()
		{
			return canExecute.Invoke();
		}

		public override bool CanExecute(object parameter)
		{
			return CanExecute();
		}

		public void Execute()
		{
			if(CanExecute()) {
				execute.Invoke();
			}
		}

		public override void Execute(object parameter)
		{
			Execute();
		}
	}
}
