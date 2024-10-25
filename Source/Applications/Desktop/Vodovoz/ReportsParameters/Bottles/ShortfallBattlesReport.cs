using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bottles;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShortfallBattlesReport : ViewBase<ShortfallBattlesReportViewModel>, ISingleUoWDialog
	{
		public ShortfallBattlesReport(ShortfallBattlesReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydatepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			comboboxDriver.ItemsEnum = ViewModel.DriverTypeType;
			comboboxDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DriverType, w => w.SelectedItem)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.AddBinding(vm => vm.OneDriver, w => w.Sensitive)
				.InitializeFromSource();

			checkOneDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OneDriver, w => w.Active)
				.InitializeFromSource();

			ySpecCmbNonReturnReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.NonReturnReason, w => w.SelectedItem)
				.AddBinding(vm => vm.NonReturnReasons, w => w.ItemsList)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
