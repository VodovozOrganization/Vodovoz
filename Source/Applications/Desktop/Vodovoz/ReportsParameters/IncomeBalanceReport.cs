using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using System.ComponentModel.DataAnnotations;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	public partial class IncomeBalanceReport : SingleUoWWidgetBase, IParametersWidget
	{
		private string reportPath = "Sales.CommonIncomeBalance";
		private readonly ReportFactory _reportFactory;

		public IncomeBalanceReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			dateperiodpicker.StartDate = DateTime.Now.Date;
			dateperiodpicker.EndDate = DateTime.Now.Date;
			yenumcomboboxReportType.ItemsEnum = typeof(IncomeReportType);
			yenumcomboboxReportType.SelectedItem = IncomeReportType.Сommon;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по приходу по кассе";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			string startDate = $"{dateperiodpicker.StartDate:yyyy-MM-dd}";
			string endDate = $"{dateperiodpicker.EndDate:yyyy-MM-dd}";

			var parameters = new Dictionary<string, object> {
				{ "StartDate", startDate },
				{ "EndDate", endDate }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = reportPath;
			reportInfo.UseUserVariables = true;
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			switch(yenumcomboboxReportType.SelectedItem as IncomeReportType?)
			{
				case IncomeReportType.ByRouteList:
					reportPath = "Sales.IncomeBalanceByMl";
					break;
				case IncomeReportType.BySelfDelivery:
					reportPath = "Sales.IncomeBalanceBySelfDelivery";
					break;
				default:
					reportPath = "Sales.CommonIncomeBalance";
					break;
			}

			OnUpdate(true);
		}
	}

	public enum IncomeReportType
	{
		[Display(Name = "Общий отчет")]
		Сommon,
		[Display(Name = "По МЛ")]
		ByRouteList,
		[Display(Name = "По Самовывозу")]
		BySelfDelivery

	}
}
