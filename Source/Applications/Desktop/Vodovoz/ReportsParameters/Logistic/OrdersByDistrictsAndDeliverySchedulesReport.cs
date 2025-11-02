using QS.Views;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersByDistrictsAndDeliverySchedulesReport : ViewBase<OrdersByDistrictsAndDeliverySchedulesReportViewModel>
	{
		public OrdersByDistrictsAndDeliverySchedulesReport(OrdersByDistrictsAndDeliverySchedulesReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			pkrDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			lstGeographicGroup.SetRenderTextFunc<GeoGroup>(x => x.Name);
			lstGeographicGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroup, w => w.SelectedItem)
				.AddBinding(vm => vm.GeoGroups, w => w.ItemsList)
				.InitializeFromSource();

			yspeccomboboxTariffZone.SetRenderTextFunc<TariffZone>(x => x.Name);
			yspeccomboboxTariffZone.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TariffZone, w => w.SelectedItem)
				.AddBinding(vm => vm.TariffZones, w => w.ItemsList)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
