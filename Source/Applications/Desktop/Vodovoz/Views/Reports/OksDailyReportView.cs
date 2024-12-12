using QS.Views.GtkUI;
using Vodovoz.ViewModels.Reports.OKS.DailyReport;

namespace Vodovoz.Views.Reports
{
	public partial class OksDailyReportView : TabViewBase<OksDailyReportViewModel>
	{
		public OksDailyReportView(OksDailyReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			datepickerReportDate.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();

			ybuttonCreate.BindCommand(ViewModel.CreateReportCommand);
		}
	}
}
