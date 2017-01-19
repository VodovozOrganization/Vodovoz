using System;
using QSOrmProject;
using QSReport;
using System.Collections.Generic;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShortfallBattlesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public ShortfallBattlesReport()
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
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

		public string Title {
			get	{
				return "Отчет о несданных бутылях";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "Orders.ShortfallBattlesReport",
				Parameters = new Dictionary<string, object>
				{ 
					{ "date", ydatepicker.Date },
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonCreateRepotClicked (object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}

