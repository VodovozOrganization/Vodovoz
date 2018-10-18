using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OnecCommentsReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public OnecCommentsReport ()
		{
			this.Build ();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject { get { return null; } }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по комментариям для логистов";
			}
		}

		#endregion

		private ReportInfo GetReportInfo ()
		{
			return new ReportInfo {
				Identifier = "Orders.OnecComments",
				UseUserVariables = true,
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) }
				}
			};
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		void OnUpdate (bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		void CanRun ()
		{
			buttonCreateReport.Sensitive =
				(dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null);
		}

		protected void OnDateperiodpickerPeriodChanged (object sender, EventArgs e)
		{
			CanRun ();
		}

	}
}
