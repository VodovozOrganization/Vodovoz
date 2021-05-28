using System;
using QS.Dialog.GtkUI;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressesOverpaymentsReport : SingleUoWWidgetBase, IParametersWidget
	{
		public AddressesOverpaymentsReport()
		{
			this.Build();
		}

		public string Title => "Отчет по переплатам за адрес";
		public event EventHandler<LoadReportEventArgs> LoadReport;
	}
}
