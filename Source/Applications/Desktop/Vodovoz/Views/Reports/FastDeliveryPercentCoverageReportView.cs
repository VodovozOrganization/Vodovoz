using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryPercentCoverageReportView : TabViewBase<FastDeliveryPercentCoverageReportViewModel>
	{
		public FastDeliveryPercentCoverageReportView(FastDeliveryPercentCoverageReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}
