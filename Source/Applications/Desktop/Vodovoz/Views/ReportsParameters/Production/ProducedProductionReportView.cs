using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Production;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Views.ReportsParameters.Production
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProducedProductionReportView : ViewBase<ProducedProductionReportViewModel>
	{
		public ProducedProductionReportView(ProducedProductionReportViewModel viewModel) : base(viewModel)
		{	
			Build();

			yenumcomboboxMonths.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Months, w => w.ItemsEnum)
				.AddBinding(vm => vm.SelectedMonth, w => w.SelectedItem)
				.InitializeFromSource();

			ylistcomboboxYear.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Years, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedYear, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboboxReportType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ReportModes, w => w.ItemsEnum)
				.AddBinding(vm => vm.SelectedReportMode, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboboxMeasurementUnit.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MeasurementUnits, w => w.ItemsEnum)
				.AddBinding(vm => vm.SelectedMeasurementUnit, w => w.SelectedItem)
				.InitializeFromSource();

			var filterView = new IncludeExludeFiltersView(viewModel.FilterViewModel);
			vboxParameters.Add(filterView);
			filterView.Show();

			buttonCreateReport.Clicked += (sender, args) => ViewModel.GenerateReportCommand.Execute();
		}

		public override void Destroy()
		{
			ViewModel?.Dispose();
			base.Destroy();
		}
	}
}
