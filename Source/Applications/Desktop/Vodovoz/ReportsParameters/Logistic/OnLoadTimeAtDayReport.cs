using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class OnLoadTimeAtDayReport : ViewBase<OnLoadTimeAtDayReportViewModel>
	{
		public OnLoadTimeAtDayReport(OnLoadTimeAtDayReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydateAtDay.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
