using Gamma.ColumnConfig;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using System;
using Vodovoz.ViewModels.ReportsParameters.Wages;
using VodovozBusiness.Nodes;

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
				.AddColumn("Категория штрафа").AddTextRenderer(x => x.FineCategoryName)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();

			ytreeviewFineCategory.Binding
				.AddBinding(ViewModel, x => x.FineCategories, x => x.ItemsDataSource)
				.InitializeFromSource();

			ytreeviewFineCategory.HeadersVisible = false;

			buttonCategoryNone.Clicked += OnCategoryNoneClicked;

			buttonCategoryAll.Clicked += OnCategoryAllClicked;

			ycheckbuttonShowArchieve.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);

		}

		private void OnCategoryAllClicked(object sender, EventArgs e)
		{
			ViewModel.AllCategoriesCommand.Execute();
			ytreeviewFineCategory.YTreeModel.EmitModelChanged();
		}

		private void OnCategoryNoneClicked(object sender, EventArgs e)
		{
			ViewModel.NoneCategoriesCommand.Execute();
			ytreeviewFineCategory.YTreeModel.EmitModelChanged();
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
