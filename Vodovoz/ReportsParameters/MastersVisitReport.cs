using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MastersVisitReport : SingleUoWWidgetBase, IParametersWidget
	{
		public MastersVisitReport(IEmployeeRepository employeeRepository)
		{
			if(employeeRepository == null)
			{
				throw new ArgumentNullException(nameof(employeeRepository));
			}
			
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			yentryrefEmployee.ItemsQuery = employeeRepository.DriversQuery();
			ButtonSensivity();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчёт по выездам мастеров";
			}
		}

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "ServiceCenter.MastersVisitReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "master", (yentryrefEmployee.Subject as Employee).Id}
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}

		void ButtonSensivity()
		{
			buttonCreateReport.Sensitive = yentryrefEmployee.Subject != null;
		}

		protected void OnYentryrefEmployeeChanged(object sender, EventArgs e)
		{
			ButtonSensivity();
		}
	}
}
