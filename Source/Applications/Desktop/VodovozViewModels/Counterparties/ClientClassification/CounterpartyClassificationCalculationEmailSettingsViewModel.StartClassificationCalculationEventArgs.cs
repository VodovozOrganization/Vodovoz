using System;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationEmailSettingsViewModel
	{
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
