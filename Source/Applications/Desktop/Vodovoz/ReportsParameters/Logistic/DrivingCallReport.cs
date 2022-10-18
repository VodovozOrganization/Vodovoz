using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DrivingCallReport : SingleUoWWidgetBase, IParametersWidget
	{
		public DrivingCallReport()
		{
			this.Build();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчёт по водительскому телефону";
			}
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("startDate", dateperiodpicker.StartDateOrNull);
			parameters.Add("endDate", dateperiodpicker.EndDateOrNull);

			return new ReportInfo {
				Identifier = "Logistic.DrivingCall",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = (dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null);
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			CanRun();
		}

		#endregion
	}
}
