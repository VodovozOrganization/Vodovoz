using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersClassificationReportView : ViewBase<UndeliveredOrdersClassificationReportViewModel>
	{
		public UndeliveredOrdersClassificationReportView(UndeliveredOrdersClassificationReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
