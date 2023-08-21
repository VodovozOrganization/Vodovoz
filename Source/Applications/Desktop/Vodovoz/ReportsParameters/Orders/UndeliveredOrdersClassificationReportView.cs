using QS.Views;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
	[ToolboxItem(true)]
	public partial class UndeliveredOrdersClassificationReportView : ViewBase<UndeliveredOrdersClassificationReportViewModel>
	{
		public UndeliveredOrdersClassificationReportView(UndeliveredOrdersClassificationReportViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
