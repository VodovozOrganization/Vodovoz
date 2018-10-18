using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DefectiveItemsReport : Gtk.Bin, IParametersWidget
	{
		IUnitOfWork uow;

		public DefectiveItemsReport()
		{
			this.Build();
			uow = UnitOfWorkFactory.CreateWithoutRoot();

			yEnumCmbSource.ItemsEnum = typeof(DefectSource);
			yEnumCmbSource.AddEnumToHideList(new Enum[] { DefectSource.None });

			var DriversFilter = new EmployeeFilter(uow);
			DriversFilter.RestrictCategory = EmployeeCategory.driver;
			yEntryRefDriver.RepresentationModel = new EmployeesVM(DriversFilter);

			datePeriod.StartDate = datePeriod.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title => "Отчёт по браку";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var driver = 0;
			if(yEntryRefDriver.Subject is Employee)
				driver = (yEntryRefDriver.Subject as Employee).Id;
			var source = yEnumCmbSource.SelectedItem;
			var startDate = datePeriod.StartDateOrNull.Value.ToString("yyyy-MM-dd");
			var endDate = datePeriod.EndDateOrNull.Value.ToString("yyyy-MM-dd");

			var repInfo = new ReportInfo {
				Identifier = "Store.DefectiveItemsReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", startDate },
					{ "end_date", endDate },
					{ "source", source },
					{ "driver_id", driver }
				}
			};

			return repInfo;
		}

		void ValidateParameters()
		{
			var datePeriodSelected = datePeriod.EndDateOrNull != null && datePeriod.StartDateOrNull != null;
			buttonRun.Sensitive = datePeriodSelected;
		}

		protected void OnDatePeriodPeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}
	}
}