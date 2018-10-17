using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ForwarderWageReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public ForwarderWageReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();

			var filterForwarder = new EmployeeFilter(UoW);
			filterForwarder.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.forwarder);
			yentryreferenceForwarder.RepresentationModel = new EmployeesVM(filterForwarder);
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

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get	{
				return "Отчет по зарплате экспедитора";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "forwarder_id", (yentryreferenceForwarder.Subject as Employee)?.Id }
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
					&& yentryreferenceForwarder.Subject != null);
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		protected void OnDateperiodpickerPeriodChanged (object sender, EventArgs e)
		{
			CanRun();
		}

		protected void OnYentryreferenceForwarderChanged (object sender, EventArgs e)
		{
			CanRun();
		}
	}
}

