using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersClassificationReport : SingleUoWWidgetBase, IParametersWidget
	{
		public UndeliveredOrdersClassificationReport()
		{
			this.Build();
		}

		public string Title => "Отчеь";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Sales.SetBillsReport",
				Parameters = new Dictionary<string, object>
				{
					{ "creationDate", DateTime.Now }
				}
			};
		}

		public void Destroy()
		{

		}
	}
}
