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
			ConfigureWidget();
		}

		private void ConfigureWidget()
		{
			yhboxInterval.Binding.AddBinding(ViewModel, vm => vm.IsIntervalVisible, w => w.Visible).InitializeFromSource();

			ySpecCmbGeographicGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroup, w => w.SelectedItem)
				.AddBinding(vm => vm.GeoGroups, w => w.ItemsList)
				.InitializeFromSource();
			
			ycheckIntervalFromCreateTime.Binding.AddBinding(ViewModel, vm => vm.IsIntervalFromOrderCreated, w => w.Active).InitializeFromSource();
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

			yhboxRouteListOwnType.Add(includeFilterView);
			includeFilterView.Show();

			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}

		private void OnButtonCreateReportClicked(object sender, System.EventArgs e) => ViewModel.GenerateReportCommand.Execute();
	}
}
