using QS.Dialog;
using System;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel
	{
		public class CalculationMessageEventArgs : EventArgs
		{
			public ImportanceLevel ImportanceLevel;
			public string ErrorMessage { get; set; }
		}
	}
}
