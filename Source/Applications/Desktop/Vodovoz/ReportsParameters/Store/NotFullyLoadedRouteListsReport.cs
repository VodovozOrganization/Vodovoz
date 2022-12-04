using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Store;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NotFullyLoadedRouteListsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public NotFullyLoadedRouteListsReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			yEntRefWarehouse.SubjectType = typeof(Warehouse);
			datePeriodPicker.StartDate = datePeriodPicker.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по не полностью погруженным МЛ";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", datePeriodPicker.StartDateOrNull.Value },
				{ "end_date", datePeriodPicker.EndDateOrNull.Value },
				{ "warehouse_id", (yEntRefWarehouse.Subject as Warehouse)?.Id ?? 0}
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Store.NotFullyLoadedRouteLists";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		protected void OnDatePeriodPickerPeriodChanged(object sender, EventArgs e)
		{
			SetSensitivity();
		}

		private void SetSensitivity()
		{
			var datePeriodSelected = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
			buttonRun.Sensitive = datePeriodSelected;
		}
	}
}
