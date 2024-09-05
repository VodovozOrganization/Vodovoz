using QS.Views;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Wages;

namespace Vodovoz.Reports
{
	[ToolboxItem(true)]
	public partial class DriverWagesReport : ViewBase<DriverWagesReportViewModel>
	{
		public DriverWagesReport(DriverWagesReportViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckbuttonShowFinesOutsidePeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowFinesOutsidePeriod, w => w.Active)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
