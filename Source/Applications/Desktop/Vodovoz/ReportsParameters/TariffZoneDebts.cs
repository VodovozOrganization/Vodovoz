using QS.Views;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ReportsParameters.QualityControl;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TariffZoneDebts : ViewBase<TariffZoneDebtsViewModel>
	{
		public TariffZoneDebts(TariffZoneDebtsViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			yspinbuttonFrom.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DebtFrom, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonTo.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DebtTo, w => w.ValueAsInt)
				.InitializeFromSource();

			yspeccomboboxTariffZone.SetRenderTextFunc<TariffZone>(x => x.Name);
			yspeccomboboxTariffZone.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TariffZones, w => w.ItemsList)
				.AddBinding(vm => vm.TariffZone, w => w.SelectedItem)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
