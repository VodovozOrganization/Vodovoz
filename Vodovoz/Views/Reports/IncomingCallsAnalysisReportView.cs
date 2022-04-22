using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Reports;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomingCallsAnalysisReportView : DialogViewBase<IncomingCallsAnalysisReportViewModel>
	{
		public IncomingCallsAnalysisReportView(IncomingCallsAnalysisReportViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
