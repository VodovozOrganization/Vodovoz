using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Store;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DefectiveItemsReport : ViewBase<DefectiveItemsReportViewModel>, ISingleUoWDialog
	{
		public DefectiveItemsReport(DefectiveItemsReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			yEnumCmbSource.ItemsEnum = ViewModel.DefectSourceType;
			yEnumCmbSource.AddEnumToHideList(ViewModel.HiddenDefectSources);
			yEnumCmbSource.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DefectSource, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			datePeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			
			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();	

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
