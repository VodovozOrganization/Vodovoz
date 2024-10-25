using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Wages;

namespace Vodovoz.Reports
{
	public partial class EmployeesFines : ViewBase<EmployeesFinesViewModel>, ISingleUoWDialog
	{
		public EmployeesFines(EmployeesFinesViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			radioCatDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CategoryDriver, w => w.Active)
				.InitializeFromSource();

			radioCatForwarder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CategoryForwarder, w => w.Active)
				.InitializeFromSource();

			radioCatOffice.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CategoryOffice, w => w.Active)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);

		}
		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
