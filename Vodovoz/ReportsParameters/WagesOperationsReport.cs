using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WagesOperationsReport: Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public WagesOperationsReport()
		{
			this.Build();

			UoW = UnitOfWorkFactory.CreateWithoutRoot ();

			var filter = new EmployeeFilter(UoW);
			filter.RestrictFired = false;
			yentryreferenceEmployee.RepresentationModel = new EmployeesVM(filter);
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по зарплатным операциям";
			}
		}

		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "Employees.WagesOperations",
				UseUserVariables = true,
				Parameters = new Dictionary<string, object>
				{ 
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
					{ "employee_id", ((Employee)yentryreferenceEmployee.Subject).Id }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if (yentryreferenceEmployee.Subject == null)
				errorString += "Не заполнено поле сотрудника\n";
			if (dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if (!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogWorks.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}
		#endregion
	}
}

