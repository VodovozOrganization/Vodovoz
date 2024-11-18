using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Selfdelivery;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveryReport : ViewBase<SelfDeliveryReportViewModel>
	{
		public SelfDeliveryReport(SelfDeliveryReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			ylabelWarningMessage.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WarningText, w => w.Text)
				.AddBinding(vm => vm.WarningVisible, w => w.Visible)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
