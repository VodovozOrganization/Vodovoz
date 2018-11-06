using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Report;
using QSProjectsLib;
using QSReport;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EquipmentReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public EquipmentReport()
		{
			this.Build();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по оборудованию";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "ServiceCenter.EquipmentReport",
				Parameters = new Dictionary<string, object>
				{ 
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
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
			if (dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if (!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogWorks.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}
	}
}

