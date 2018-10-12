using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MastersVisitReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		IUnitOfWork uow;

		public MastersVisitReport()
		{
			this.Build();
			uow = UnitOfWorkFactory.CreateWithoutRoot();
			yentryrefEmployee.ItemsQuery = EmployeeRepository.DriversQuery();
			ButtonSensivity();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get { return uow; } private set {; } }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

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
				MessageDialogWorks.RunErrorDialog("Необходимо выбрать дату");
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
