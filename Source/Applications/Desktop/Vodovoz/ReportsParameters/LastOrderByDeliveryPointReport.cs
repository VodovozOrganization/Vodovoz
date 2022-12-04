using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QSWidgetLib;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	public partial class LastOrderByDeliveryPointReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public LastOrderByDeliveryPointReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
			BottleDeptEntry.ValidationMode = ValidationType.numeric;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по последнему заказу";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			bool isSortByBottles;
			int deptCount;
			if(!String.IsNullOrEmpty(BottleDeptEntry.Text)) {
				deptCount = Convert.ToInt32(BottleDeptEntry.Text);
				isSortByBottles = true;
			} 
			else {
				isSortByBottles = false;
				deptCount = 0;
			}
			var parameters = new Dictionary<string, object>
			{
				{ "date", ydatepicker.Date },
				{ "bottles_count", deptCount},
				{ "is_sort_bottles", isSortByBottles }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = buttonSanitary.Active ? "Orders.SanitaryReport" : "Orders.OrdersByDeliveryPoint";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
