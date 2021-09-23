using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalyticsForUndeliveryReport : SingleUoWWidgetBase, IParametersWidget
	{
		private int itogLO = 0;
		private string titleDate;
		
		public AnalyticsForUndeliveryReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}
		
		void ConfigureDlg()
		{
			pkrDate.StartDate = pkrDate.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation
		
		public string Title => "Аналитика по недовозам";

		public event EventHandler<LoadReportEventArgs> LoadReport;
		
		#endregion
		
		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			GetGuilty();
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			int[] geoparts = {1,2,3};
			return new ReportInfo {
				Identifier = "Logistic.AnalyticsForUndelivery",
				Parameters = new Dictionary<string, object> {
					{ "start_date", pkrDate.StartDate },
					{ "end_date", pkrDate.EndDate },
					{"title_date",titleDate},
					{"geoparts",geoparts}
				}
			};
		}
		
		public void GetGuilty()
		{
			if(pkrDate == null)
			{
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не заполнена дата!");
			}
			else
			{
				titleDate = pkrDate.StartDate.ToShortDateString();
			}

			if(pkrDate.EndDate != null && pkrDate.EndDate != pkrDate.StartDate)
			{
				titleDate = titleDate + " и на " + pkrDate.EndDate.ToShortDateString();
			}

		}
	}
}
