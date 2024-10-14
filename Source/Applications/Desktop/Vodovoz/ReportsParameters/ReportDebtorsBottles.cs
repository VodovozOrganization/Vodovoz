using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bottles;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReportDebtorsBottles : ViewBase<ReportDebtorsBottlesViewModel>
	{
		public ReportDebtorsBottles(ReportDebtorsBottlesViewModel viewModel) : base(viewModel)
		{
			this.Build();

			radiobuttonAllShow.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowAll, w => w.Active)
				.InitializeFromSource();

			radiobuttonNotManualEntered.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.NotManualEntered, w => w.Active)
				.InitializeFromSource();

			radiobuttonOnlyManualEntered.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OnlyManualEntered, w => w.Active)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
