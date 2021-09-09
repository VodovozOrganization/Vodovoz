using System;
using QS.Dialog.GtkUI;
using QSReport;

namespace Vodovoz.ReportsParameters.Sales
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalaryRatesReport : SingleUoWWidgetBase, IParametersWidget
	{
		public SalaryRatesReport()
		{
			this.Build();
		}

		public string Title => "Ставки для водителей";

		public event EventHandler<LoadReportEventArgs> LoadReport;
	}
}
