using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeesPremiums : SingleUoWWidgetBase, IParametersWidget
	{
		public EmployeesPremiums()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var employeeFactory = new EmployeeJournalFactory();
			evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeDriver.Changed += (sender, args) => ValidateParameters();
		}

		#region IParametersWidget implementation

		public string Title => "Премии сотрудников";

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

			if(evmeDriver.Subject != null)
				parameters.Add("drivers", evmeDriver.SubjectId);
			else {
				parameters.Add("drivers", -1);
			}
			if(dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null) {
				parameters.Add("startDate", dateperiodpicker1.StartDateOrNull.Value);
				parameters.Add("endDate", dateperiodpicker1.EndDateOrNull.Value);
			} else {
				parameters.Add("startDate", 0);
				parameters.Add("endDate", 0);
			}

			parameters.Add("showbottom", false);
			parameters.Add("category", GetCategory());

			return new ReportInfo {
				Identifier = "Employees.Premiums",
				Parameters = parameters
			};
		}

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var datePeriodSelected = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
			var driverSelected = evmeDriver.Subject != null;
			buttonRun.Sensitive = datePeriodSelected || driverSelected;
		}

		protected string GetCategory()
		{
			string cat = "-1";

			if(radioCatDriver.Active) {
				cat = EmployeeCategory.driver.ToString();
			} else if(radioCatForwarder.Active) {
				cat = EmployeeCategory.forwarder.ToString();
			} else if(radioCatOffice.Active) {
				cat = EmployeeCategory.office.ToString();
			}

			return cat;
		}
	}
}
