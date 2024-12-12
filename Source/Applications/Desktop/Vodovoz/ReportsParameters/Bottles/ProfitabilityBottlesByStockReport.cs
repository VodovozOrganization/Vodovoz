using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bottles;

namespace Vodovoz.ReportsParameters.Bottles
{
	public partial class ProfitabilityBottlesByStockReport : ViewBase<ProfitabilityBottlesByStockReportViewModel>
	{
		public ProfitabilityBottlesByStockReport(ProfitabilityBottlesByStockReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dtrngPeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			specCmbDiscountPct.SetRenderTextFunc<ProfitabilityBottlesByStockReportViewModel.PercentNode>(x => x.Name);
			specCmbDiscountPct.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PercentNodes, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedPercentNode, w => w.SelectedItem)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
