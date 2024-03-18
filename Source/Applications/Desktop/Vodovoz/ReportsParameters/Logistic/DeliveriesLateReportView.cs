using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistic;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Views.ReportsParameters.Logistic
{
	public partial class DeliveriesLateReportView : ViewBase<DeliveriesLateReportViewModel>
	{
		public DeliveriesLateReportView(DeliveriesLateReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yhboxInterval.Binding.AddBinding(ViewModel, vm => vm.IsIntervalVisible, w => w.Visible).InitializeFromSource();

			ySpecCmbGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeoGroup, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeoGroups, w => w.ItemsList).InitializeFromSource();
			
			ycheckIntervalFromFirstAddress.Binding.AddBinding(ViewModel, vm => vm.IsIntervalFromFirstAddress, w => w.Active).InitializeFromSource();
			ycheckIntervalFromTransferTime.Binding.AddBinding(ViewModel, vm => vm.IsIntervalFromTransferTime, w => w.Active).InitializeFromSource();

			ycheckOnlyFastSelect.Binding.AddBinding(ViewModel, vm => vm.IsOnlyFastSelect, w => w.Active).InitializeFromSource();
			ycheckWithoutFastSelect.Binding.AddBinding(ViewModel, vm => vm.IsWithoutFastSelect, w => w.Active).InitializeFromSource();
			ycheckAllSelect.Binding.AddBinding(ViewModel, vm => vm.AllOrderSelect, w => w.Active).InitializeFromSource();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			ychkDriverSort.Binding.AddBinding(ViewModel, vm => vm.IsDriverSort, w => w.Active).InitializeFromSource();

			var includeFilterView = new IncludeExludeFiltersView(ViewModel.IncludeFilterViewModel);

			vbox1.Add(includeFilterView);
			includeFilterView.Show();

			buttonCreateReport.Clicked += (sender, args) => ViewModel.GenerateReportCommand.Execute();
		}
	}
}
