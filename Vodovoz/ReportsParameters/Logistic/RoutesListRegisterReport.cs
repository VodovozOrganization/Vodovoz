using System;
using System.Collections.Generic;
using QSProjectsLib;
using QSReport;

namespace Vodovoz.Reports.Logistic
{
	public partial class RoutesListRegisterReport : Gtk.Bin, IParametersWidget
	{
		public RoutesListRegisterReport ()
		{
			this.Build ();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Реестр маршрутных листов";
			}
		}

		#endregion

		void OnUpdate (bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		private ReportInfo GetReportInfo ()
		{
			return new ReportInfo {
				Identifier = "Logistic.RoutesListRegister",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull }
				}
			};
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			if (dateperiodpicker.StartDateOrNull == null) {
				MessageDialogWorks.RunErrorDialog ("Необходимо выбрать дату");
				return;
			}
			OnUpdate (true);
		}
	}
}
