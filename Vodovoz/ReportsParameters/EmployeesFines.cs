using System;
using QSReport;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Reports
{
	public partial class EmployeesFines : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public EmployeesFines()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			yentryDriver.SubjectType = typeof(Employee);
		}
		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

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
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			if(yentryDriver.Subject != null)
				parameters.Add("drivers", (yentryDriver.Subject as Employee).Id);
			else {
				parameters.Add("drivers", -1);
			}
			if(dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null) {
				parameters.Add("startDate", dateperiodpicker1.StartDateOrNull.Value);
				parameters.Add("endDate", dateperiodpicker1.EndDateOrNull.Value);
			}else{
				parameters.Add("startDate", 0);
				parameters.Add("endDate", 0);
			}
			return new ReportInfo
			{
				Identifier = "Employees.Fines",
				Parameters = parameters
			};
		}			

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		protected void OnYentryDriverChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var datePeriodSelected = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
			var driverSelected = yentryDriver.Subject != null;
			buttonRun.Sensitive = datePeriodSelected || driverSelected;
		}


	}
}

