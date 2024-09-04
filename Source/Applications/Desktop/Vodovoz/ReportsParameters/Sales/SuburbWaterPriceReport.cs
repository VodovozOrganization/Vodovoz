using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Sales;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class SuburbWaterPriceReport : ViewBase<SuburbWaterPriceReportViewModel>
	{
		public SuburbWaterPriceReport(SuburbWaterPriceReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydatepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
