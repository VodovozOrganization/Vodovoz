using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveryReport : SingleUoWWidgetBase, IParametersWidget
	{
		private const int REPORT_MAX_PERIOD = 62;
		private readonly ReportFactory _reportFactory;

		public SelfDeliveryReport(ReportFactory reportFactory)
		{
			this.Build();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			dateperiodpicker.StartDate = DateTime.Now.Date;
			dateperiodpicker.EndDate = DateTime.Now.Date;
			dateperiodpicker.PeriodChanged += DateperiodpickerPeriodChanged;
			this.ylabelWarningMessage.Visible = false;
		}

		private void DateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			if (dateperiodpicker.StartDate.Date.AddDays(REPORT_MAX_PERIOD - 1) < dateperiodpicker.EndDate.Date)
			{
				this.ylabelWarningMessage.Visible = true;
				this.ylabelWarningMessage.Text = $"Выбран период более {REPORT_MAX_PERIOD} дней";
				this.buttonCreateRepot.Sensitive = false;
			}
			else
			{
				this.ylabelWarningMessage.Visible = false;
				this.buttonCreateRepot.Sensitive = true;
			}
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по самовывозу";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			. return new ReportInfo {
				Identifier = "Orders.SelfDeliveryReport",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker.StartDate },
					{ "endDate", dateperiodpicker.EndDate },
					{ "isOneDayReport", dateperiodpicker.StartDate.Date == dateperiodpicker.EndDate.Date }
				}
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.SelfDeliveryReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
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
