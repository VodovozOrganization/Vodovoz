using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Report;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class SuburbWaterPriceReport : Gtk.Bin, IEntityDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по ценам пригорода";
			}
		}


		public SuburbWaterPriceReport()
		{
			this.Build();
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Sales.SuburbWaterPrice",
				ParameterDatesWithTime = false,
				Parameters = new Dictionary<string, object>
				{
					{ "report_date", ydatepicker.Date }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
