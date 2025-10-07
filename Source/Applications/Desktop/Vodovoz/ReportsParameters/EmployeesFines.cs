using Gamma.ColumnConfig;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.ReportsParameters.Wages;
using Gamma.Utilities;

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
			
			ytreeviewFineCategory.ColumnsConfig = FluentColumnsConfig<EmployeeFineCategoryNode>.Create()
				.AddColumn("Категория штрафа").AddTextRenderer(x => x.FineCategory.GetEnumTitle())
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();

			ytreeviewFineCategory.Binding
				.AddBinding(ViewModel, x => x.FineCategories, x => x.ItemsDataSource)
				.InitializeFromSource();

			ytreeviewFineCategory.HeadersVisible = false;

			buttonStatusNone.Clicked += (sender, args) => ViewModel.NoneStatusCommand.Execute();

			buttonStatusAll.Clicked += (sender, args) => ViewModel.AllStatusCommand.Execute();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);

		}
		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
