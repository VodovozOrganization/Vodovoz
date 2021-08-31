using System;
using QS.Dialog.GtkUI;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalyticsForUndeliveryReport : SingleUoWWidgetBase, IParametersWidget
	{
		public AnalyticsForUndeliveryReport()
		{
			this.Build();
		}

		public string Title => throw new NotImplementedException();

		public event EventHandler<LoadReportEventArgs> LoadReport;
	}
}
