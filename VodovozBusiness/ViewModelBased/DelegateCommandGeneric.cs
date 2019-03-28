using System;
namespace Vodovoz.ViewModelBased
{
	public class DelegateCommand<TParameter> : PropertySubscribedCommandBase
	{
		private readonly Predicate<TParameter> canExecute;
		private readonly Action<TParameter> execute;

		public DelegateCommand(Action<TParameter> execute, Predicate<TParameter> canExecute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public DelegateCommand(Action<TParameter> execute) : this(execute, null)
		{
		}

		public override bool CanExecute(object parameter)
		{
			if(!(parameter is TParameter)) {
				return false;
			}
			return CanExecute((TParameter)parameter);
		}

		public override void Execute(object parameter)
		{
			if(!CanExecute(parameter))
				return;

			Execute((TParameter)parameter);
		}

		public bool CanExecute(TParameter parameter)
		{
			return canExecute == null || canExecute(parameter);
		}

		public void Execute(TParameter parameter)
		{
			if(!CanExecute(parameter))
				return;

			execute(parameter);
		}
	}
}
