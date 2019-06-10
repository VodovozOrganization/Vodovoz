using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
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

			repEntryEmployee.RepresentationModel = new EmployeesVM();
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
					{ "employee_id", ((Employee)repEntryEmployee.Subject).Id }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if (repEntryEmployee.Subject == null)
				errorString += "Не заполнено поле сотрудника\n";
			if (dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if (!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogHelper.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}
		#endregion
	}
}

