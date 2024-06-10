using Gamma.ColumnConfig;
using QS.Dialog;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistic;
using static Vodovoz.ViewModels.ReportsParameters.Logistic.DeliveryTimeReportViewModel;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReportView : ViewBase<DeliveryTimeReportViewModel>
	{
		private readonly IInteractiveService interactiveService;

		public DeliveryTimeReportView(DeliveryTimeReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytreeGeoGroups.HeadersVisible = false;
			ytreeGeoGroups.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();

			ytreeGeoGroups.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroupList, w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeRouteListTypeOfUse.HeadersVisible = false;
			ytreeRouteListTypeOfUse.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("").AddEnumRenderer(x => x.RouteListOwnType)
				.Finish();
			ytreeRouteListTypeOfUse.ItemsDataSource = ViewModel.RouteListTypeOfUseList;

			ytimeDelivery.Binding.AddBinding(ViewModel, vm => vm.Time, w => w.Time).InitializeFromSource();

			speciallistcomboboxOrdersEnRouteCount.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OrdersEnRouteCountList, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedOrdersEnRouteCount, w => w.SelectedItem)
				.InitializeFromSource();

			comboboxReportType.ItemsEnum = typeof(ReportType);
			comboboxReportType.Binding
				.AddBinding(ViewModel, vm => vm.SelectedReportType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			buttonCreateReport.Clicked += (sender, args) => ViewModel.GenerateReportCommand.Execute();
		}
	}
}
