using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Wages;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WagesOperationsReport : ViewBase<WagesOperationsReportViewModel>, ISingleUoWDialog
	{
		public WagesOperationsReport(WagesOperationsReportViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			evmeEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeEmployee.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Employee, w => w.Subject)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
