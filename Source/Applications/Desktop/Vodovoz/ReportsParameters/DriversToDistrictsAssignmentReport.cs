using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Services;

namespace Vodovoz.ReportsParameters
{
	public partial class DriversToDistrictsAssignmentReport : SingleUoWWidgetBase, IParametersWidget
	{
		public DriversToDistrictsAssignmentReport()
		{
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по распределению водителей на районы";

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Logistic.DriversToDistrictsAssignmentReport",
				UseUserVariables = true,
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) },
					{ "only_different_districts", onlyDifferentDistricts.Active }
				}
			};
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e) =>
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));

		private void CanRun()
		{
			buttonCreateReport.Sensitive = dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null;
		}

		private void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			CanRun();
		}
	}
}
