using System;
using QSReport;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Reports
{
	public partial class StockMovements : Gtk.Bin, IParametersWidget
	{
		IUnitOfWork uow;

		public StockMovements()
		{
			this.Build();
			uow = UnitOfWorkFactory.CreateWithoutRoot();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				yentryrefWarehouse.Subject =  CurrentUserSettings.Settings.DefaultWarehouse;
			dateperiodpicker1.StartDate = dateperiodpicker1.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title
		{
			get
			{
				return "Отчет по складу";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{			
			var wagons = Repository.Store.WagonRepository.UsedWagonsByPeriod(uow,
				             dateperiodpicker1.StartDate,
				             dateperiodpicker1.EndDate,
				             yentryrefWarehouse.Subject as Warehouse
			             );
			int wagon1 = wagons.Count > 0 ? wagons[0].Id : -1;
			int wagon2 = wagons.Count > 1 ? wagons[1].Id : -1;
			int wagon3 = wagons.Count > 2 ? wagons[2].Id : -1;
			int wagon4 = wagons.Count > 3 ? wagons[3].Id : -1;
			return new ReportInfo
			{
				Identifier = "Store.SummaryMovements",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
					{ "warehouse_id", (yentryrefWarehouse.Subject as Warehouse).Id},
					{ "wagon1_id", wagon1},
					{ "wagon2_id", wagon2},
					{ "wagon3_id", wagon3},
					{ "wagon4_id", wagon4},
				}
			};
		}			

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var datePeriodSelected = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
			var warehouse = yentryrefWarehouse.Subject != null;
			buttonRun.Sensitive = datePeriodSelected && warehouse;
		}

		protected void OnYentryrefWarehouseChangedByUser(object sender, EventArgs e)
		{
			ValidateParameters();
		}
	}
}

