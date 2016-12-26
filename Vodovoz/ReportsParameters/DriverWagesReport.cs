using System;
using QSOrmProject;
using QSReport;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using QSProjectsLib;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverWagesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public DriverWagesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			yentryreferenceDriver.ItemsQuery = Vodovoz.Repository.EmployeeRepository.DriversQuery();
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
			return new ReportInfo
			{
				Identifier = "Wages.DriverWage",
				Parameters = new Dictionary<string, object>
				{ 
					{ "driver_id", (yentryreferenceDriver.Subject as Employee).Id},
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull }
				}
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

