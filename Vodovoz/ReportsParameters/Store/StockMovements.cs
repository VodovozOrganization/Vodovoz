using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSOrmProject.RepresentationModel;
using QSReport;
using Vodovoz.Domain.Store;
using Vodovoz.FilterViewModels.Warehouses;
using Vodovoz.JournalViewModels.Warehouses;

namespace Vodovoz.Reports
{
	public partial class StockMovements : SingleUoWWidgetBase, IParametersWidget
	{
		public StockMovements()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			entityVMentryWarehouse.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Warehouse, WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(ServicesConfig.CommonServices));
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
				entityVMentryWarehouse.Subject = CurrentUserSettings.Settings.DefaultWarehouse;
			dateperiodpicker1.StartDate = dateperiodpicker1.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title => "Складские движения";

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
			string reportId;
			var warehouse = entityVMentryWarehouse.Subject as Warehouse;
			if(warehouse == null)
				reportId = "Store.StockWaterMovements";
			else if(warehouse.TypeOfUse == WarehouseUsing.Shipment)
				reportId = "Store.StockShipmentMovements";
			else if(warehouse.TypeOfUse == WarehouseUsing.Production)
				reportId = "Store.StockProductionMovements";
			else
				throw new NotImplementedException("Неизвестный тип использования склада.");

			return new ReportInfo
			{
				Identifier = reportId,
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
					{ "warehouse_id", warehouse?.Id ?? -1},
					{ "creationDate", DateTime.Now}
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
			buttonRun.Sensitive = datePeriodSelected;
		}
	}
}

