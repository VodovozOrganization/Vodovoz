using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ForwarderWageReport : Gtk.Bin, IParametersWidget
	{
		public ForwarderWageReport()
		{
			this.Build();
			var driverFilter = new EmployeeFilterViewModel
			{
				RestrictCategory = EmployeeCategory.forwarder,
				Status = EmployeeStatus.IsWorking
			};
			var employeeFactory = new EmployeeJournalFactory(driverFilter);
			evmeForwarder.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeForwarder.Changed += (sender, e) => CanRun();
			dateperiodpicker.PeriodChanged += (sender, e) => CanRun();
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по зарплате экспедитора";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "forwarder_id", evmeForwarder.SubjectId }
			};

			if(checkShowBalance.Active) {
				parameters.Add("showbalance", "1");
			} else {
				parameters.Add("showbalance", "0");
			}

			return new ReportInfo {
				Identifier = "Employees.ForwarderWage",
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = 
				(dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null
					&& evmeForwarder.Subject != null);
		}
	}
}
