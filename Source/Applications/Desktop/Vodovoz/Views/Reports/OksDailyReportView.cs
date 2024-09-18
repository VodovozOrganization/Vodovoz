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
			datepicker1.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();

			datepicker1.IsEditable = true;

			ybuttonCreate.BindCommand(ViewModel.CreateReportCommand);
		}
	}
}
