using System;
using QSOrmProject;
using QSReport;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using QSProjectsLib;
using Vodovoz.ViewModel;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverWagesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public DriverWagesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			var filter = new EmployeeFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentryreferenceDriver.RepresentationModel = new EmployeesVM(filter);
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get	{
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title	{ 
			get {
				return "Зарплата водителя";
			}
		}

		#endregion

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		private ReportInfo GetReportInfo()
		{
			var endDate = dateperiodpicker.EndDateOrNull;
			if(endDate != null)
			{
				endDate = endDate.GetValueOrDefault().AddHours(23).AddMinutes(59);
			}

			var parameters = new Dictionary<string, object>
				{
					{ "driver_id", (yentryreferenceDriver.Subject as Employee).Id},
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", endDate }
			};

			if(checkShowBalance.Active) {
				parameters.Add("showbalance", "1");
			} else {
				parameters.Add("showbalance", "0");
			}
			return new ReportInfo
			{
				Identifier = "Wages.DriverWage",
				Parameters = parameters
			};
		}	

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			if ((yentryreferenceDriver.Subject as Employee) == null)
			{
				MessageDialogWorks.RunErrorDialog("Необходимо выбрать водителя");
				return;
			}
			if(dateperiodpicker.StartDateOrNull == null)
			{
				MessageDialogWorks.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}
	}
}

