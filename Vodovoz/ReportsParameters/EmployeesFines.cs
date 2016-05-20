using System;
using QSReport;
using System.Collections.Generic;

namespace Vodovoz.Reports
{
	public partial class EmployeesFines : Gtk.Bin, IParametersWidget
	{
		public EmployeesFines()
		{
			this.Build();
		}

		#region IParametersWidget implementation

		public string Title
		{
			get
			{
				return "Штрафы сотрудников";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "Employees.Fines",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
				}
			};
		}			

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var datePeriodSelected = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
			buttonRun.Sensitive = datePeriodSelected;
		}

	}
}

