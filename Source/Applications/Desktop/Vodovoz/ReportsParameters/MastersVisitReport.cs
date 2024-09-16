using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Service;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MastersVisitReport : ViewBase<MastersVisitReportViewModel>, ISingleUoWDialog
	{
		public MastersVisitReport(MastersVisitReportViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			evmeEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.MasterSelectorFactory);
			evmeEmployee.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Master, w => w.Subject)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
