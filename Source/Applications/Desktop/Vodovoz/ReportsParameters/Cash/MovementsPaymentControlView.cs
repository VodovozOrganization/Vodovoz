using QS.Views;
using System.ComponentModel;
using Vodovoz.ViewModels.Cash.Reports;

namespace Vodovoz.ReportsParameters.Cash
{
	[ToolboxItem(true)]
	public partial class MovementsPaymentControlView : ViewBase<MovementsPaymentControlViewModel>
	{
		public MovementsPaymentControlView(MovementsPaymentControlViewModel viewModel) : base(viewModel)
		{
			Build();
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
