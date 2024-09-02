﻿using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.TempAdapters;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WagesOperationsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public WagesOperationsReport(IEmployeeJournalFactory employeeJournalFactory, ReportFactory reportFactory)
		{
			if(employeeJournalFactory is null)
			{
				throw new ArgumentNullException(nameof(employeeJournalFactory));
			}
			
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по зарплатным операциям";

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDate },
				{ "end_date", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
				{ "employee_id", evmeEmployee.SubjectId }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Employees.WagesOperations";
			reportInfo.Parameters = parameters;
			reportInfo.UseUserVariables = true;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if (evmeEmployee.Subject == null)
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
