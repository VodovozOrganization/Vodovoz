using QS.Views.Dialog;
using Vodovoz.ViewModels.Counterparties.CounterpartyClassification;

namespace Vodovoz.Views.Client.CounterpartyClassification
{
	[WindowSize(300, 400)]
	public partial class CounterpartyClassificationCalculationEmailSettingsView : DialogViewBase<CounterpartyClassificationCalculationEmailSettingsViewModel>
	{
		public CounterpartyClassificationCalculationEmailSettingsView(
			CounterpartyClassificationCalculationEmailSettingsViewModel viewModel
			) : base(viewModel)
		{
			Build();
		}
	}
}
