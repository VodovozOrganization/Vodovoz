using QS.Navigation;
using QS.ViewModels.Dialog;
using System;

namespace Vodovoz.ViewModels.Counterparties.CounterpartyClassification
{
	public class CounterpartyClassificationCalculationSettingsViewModel : WindowDialogViewModelBase
	{
		private string _userEmail;
		private string _emailForReportCopy;

		public CounterpartyClassificationCalculationSettingsViewModel(
			INavigationManager navigation,
			string userEmail
			) : base(navigation)
		{
			if(string.IsNullOrWhiteSpace(userEmail))
			{
				throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or whitespace.", nameof(userEmail));
			}

			_userEmail = userEmail;
		}

		public string UserEmail
		{
			get => _userEmail;
			set => SetField(ref _userEmail, value);
		}

		public string EmailForReportCopy
		{
			get => _emailForReportCopy;
			set => SetField(ref _emailForReportCopy, value);
		}
	}
}
