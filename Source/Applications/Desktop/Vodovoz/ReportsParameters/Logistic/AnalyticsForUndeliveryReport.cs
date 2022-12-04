using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalyticsForUndeliveryReport : SingleUoWWidgetBase, IParametersWidget
	{
		private string titleDate;
		private readonly ReportFactory _reportFactory;

		public AnalyticsForUndeliveryReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title => "Аналитика по недовозам";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			GetGuilty();
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			int[] geoparts = { 1, 2, 3 };

			var parameters = new Dictionary<string, object>
			{
				{ "first_date", dateperiodpicker.StartDate },
				{ "second_date", dateperiodpicker.EndDate },
				{ "title_date", titleDate },
				{ "geoparts", geoparts }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Logistic.AnalyticsForUndelivery";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		public void GetGuilty()
		{
			if(dateperiodpicker == null)
			{
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не заполнена дата!");
			}
			else
			{
				titleDate = dateperiodpicker.StartDate.ToShortDateString();
			}

			if(dateperiodpicker.EndDate != null && dateperiodpicker.EndDate != dateperiodpicker.StartDate)
			{
				titleDate = titleDate + " и на " + dateperiodpicker.EndDate.ToShortDateString();
			}

		}
	}
}
