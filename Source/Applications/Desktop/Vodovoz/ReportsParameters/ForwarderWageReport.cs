using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Wages;

namespace Vodovoz.Reports
{
	[ToolboxItem(true)]
	public partial class ForwarderWageReport : ViewBase<ForwarderWageReportViewModel>, ISingleUoWDialog
	{
		public ForwarderWageReport(ForwarderWageReportViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckbuttonShowFinesOutsidePeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowFinesOutsidePeriod, w => w.Active)
				.InitializeFromSource();

			checkShowBalance.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowBalance, w => w.Active)
				.InitializeFromSource();

			evmeForwarder.SetEntityAutocompleteSelectorFactory(ViewModel.ForwarderSelectorFactory);
			evmeForwarder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Forwarder, w => w.Subject)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
