using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottlesMovementReport : SingleUoWWidgetBase, IParametersWidget
	{
		public BottlesMovementReport()
		{
			this.Build();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчёт по движению бутылей (по МЛ)";

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Bottles.BottlesMovementReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull }
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать период");
				return;
			}
			OnUpdate(true);
		}
	}
}
