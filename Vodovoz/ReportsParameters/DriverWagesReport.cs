using System;
using QSOrmProject;
using QSReport;
using System.Collections.Generic;

namespace ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverWagesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public DriverWagesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
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
					{ "driver_id", dateperiodpicker.StartDateOrNull }
				}
			};
		}	
	}
}

