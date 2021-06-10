using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Reports;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryAnalyticsReportView : DialogViewBase<DeliveryAnalyticsViewModel>
	{
		public DeliveryAnalyticsReportView(DeliveryAnalyticsViewModel viewModel) : base (viewModel)
		{
			this.Build();

		}

		protected void OnExportBtnClicked(object sender, EventArgs e)
		{
		}

	}
}
