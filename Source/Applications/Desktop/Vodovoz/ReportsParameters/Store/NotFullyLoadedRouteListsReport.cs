using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Store;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NotFullyLoadedRouteListsReport : ViewBase<NotFullyLoadedRouteListsReportViewModel>
	{
		public NotFullyLoadedRouteListsReport(NotFullyLoadedRouteListsReportViewModel viewModel) : base(viewModel)
		{
			Build();

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			warehouseEntry.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WarehouseEntryViewModel, w => w.ViewModel)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
