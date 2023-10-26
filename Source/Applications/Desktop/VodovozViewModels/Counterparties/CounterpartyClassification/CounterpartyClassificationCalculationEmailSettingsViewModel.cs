using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;

namespace Vodovoz.ViewModels.Counterparties.CounterpartyClassification
{
	public class CounterpartyClassificationCalculationEmailSettingsViewModel : WindowDialogViewModelBase
	{
		private string _currentUserEmail;
		private string _additionalEmail;

		public event EventHandler<StartClassificationCalculationEventArgs> StartClassificationCalculationClicked;

		public CounterpartyClassificationCalculationEmailSettingsViewModel(
			INavigationManager navigation,
			string userEmail
			) : base(navigation)
		{
			if(string.IsNullOrWhiteSpace(userEmail))
			{
				throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or whitespace.", nameof(userEmail));
			}

			Title = "Пересчёт классификации";

			_currentUserEmail = userEmail;
		}

		#region Properties

		public string CurrentUserEmail
		{
			get => _currentUserEmail;
			set => SetField(ref _currentUserEmail, value);
		}

		public string AdditionalEmail
		{
			get => _additionalEmail;
			set => SetField(ref _additionalEmail, value);
		}

		#endregion Properties

		#region Commands
		#region StartCalculation
		private DelegateCommand _startCalculationCommand;
		public DelegateCommand StartCalculationCommand
		{
			get
			{
				if(_startCalculationCommand == null)
				{
					_startCalculationCommand = new DelegateCommand(StartCalculation, () => CanStartCalculation);
					_startCalculationCommand.CanExecuteChangedWith(this, x => x.CanStartCalculation);
				}
				return _startCalculationCommand;
			}
		}

		public bool CanStartCalculation => true;

		private void StartCalculation()
		{
			var currentUserEmail = _currentUserEmail;
			var additionalEmail = _additionalEmail ?? string.Empty;

			var clickedEventArgs = new StartClassificationCalculationEventArgs(currentUserEmail, additionalEmail);

			StartClassificationCalculationClicked?.Invoke(this, clickedEventArgs);

			this.Close(false, CloseSource.Self);
		}
		#endregion StartCalculation
		#endregion Commands

		public class StartClassificationCalculationEventArgs : EventArgs
		{
			public string CurrentUserEmail { get; }
			public string AdditionalEmail { get; }

			public StartClassificationCalculationEventArgs(string currentUserEmail, string additionalEmail)
			{
				CurrentUserEmail = currentUserEmail;
				AdditionalEmail = additionalEmail;
			}
		}
	}
}
