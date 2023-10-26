using QS.Views.GtkUI;
using Vodovoz.ViewModels.Counterparties.CounterpartyClassification;

namespace Vodovoz.Views.Client.CounterpartyClassification
{
	public partial class CounterpartyClassificationCalculationView : TabViewBase<CounterpartyClassificationCalculationViewModel>
	{
		public CounterpartyClassificationCalculationView(CounterpartyClassificationCalculationViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
