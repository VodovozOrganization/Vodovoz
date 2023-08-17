using QS.Views;
using Vodovoz.ViewModels.Cash.Reports;

namespace Vodovoz.ReportsParameters.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MovementsPaymentControlView : ViewBase<MovementsPaymentControlViewModel>
	{
		public MovementsPaymentControlView(MovementsPaymentControlViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			dateperiodpicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			buttonCreateReport.Clicked += (s, e) => ViewModel.CreateReportCommand?.Execute();
		}
	}
}
