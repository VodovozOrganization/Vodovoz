using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProductionRequestReport : SingleUoWWidgetBase, IParametersWidget
	{
		private GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; set; }

		public ProductionRequestReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		private void Configure()
		{
			buttonRun.Clicked += (sender, args) => OnUpdate();

			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			yentryrefWarehouse.ChangedByUser += YentryrefWarehouseChangedByUser;

			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
				yentryrefWarehouse.Subject = CurrentUserSettings.Settings.DefaultWarehouse;

			dateperiodpickerMaxSales.StartDate = DateTime.Today.AddYears(-1);
			dateperiodpickerMaxSales.EndDate = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
			dateperiodpickerMaxSales.PeriodChangedByUser += DateperiodpickerMaxSalesPeriodChangedByUser;

			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>(
				UoW.GetAll<GeographicGroup>().Select(x => new GeographicGroupNode(x)).ToList());
			
			GeographicGroup employeeGeographicGroup = EmployeeSingletonRepository.GetInstance()
				.GetEmployeeForCurrentUser(UoW).Subdivision.GetGeographicGroup();
			
			if(employeeGeographicGroup != null) {
				var foundGeoGroup = GeographicGroupNodes.FirstOrDefault(x => x.GeographicGroup.Id == employeeGeographicGroup.Id);

				if(foundGeoGroup != null)
					foundGeoGroup.Selected = true;
			}
			
			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
					.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing()
					.AddColumn("Район города").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();
			
			ytreeviewGeographicGroup.ItemsDataSource = GeographicGroupNodes;
			ytreeviewGeographicGroup.HeadersVisible = false;
		}

		void DateperiodpickerMaxSalesPeriodChangedByUser(object sender, EventArgs e) => 
			buttonRun.Sensitive = CanRun();

		void YentryrefWarehouseChangedByUser(object sender, EventArgs e) =>
			buttonRun.Sensitive = CanRun();

		private bool CanRun()
		{
			var gGroups = GeographicGroupNodes.Where(x => x.Selected);
			
			if(dateperiodpickerMaxSales.StartDateOrNull.HasValue
							&& dateperiodpickerMaxSales.EndDateOrNull.HasValue
							&& yentryrefWarehouse.Subject != null
							&& gGroups.Any())
				return true;

			return false;
		}

		#region IParametersWidget implementation

		public string Title => "Заявка на производство";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			string reportId;
			var warehouse = yentryrefWarehouse.Subject as Warehouse;
			
			if(warehouse != null && warehouse.TypeOfUse == WarehouseUsing.Shipment)
				reportId = "Store.ProductionRequestReport";
			else
				throw new NotImplementedException("Неизвестный тип использования склада.");

			var gGroups = GeographicGroupNodes.Where(x => x.Selected);

			var parameters = new Dictionary<string, object>
			{
				{"start_date", dateperiodpickerMaxSales.StartDateOrNull.Value},
				{"end_date", dateperiodpickerMaxSales.EndDateOrNull.Value.AddHours(23).AddMinutes(59).AddSeconds(59)},
				{"today", DateTime.Today},
				{"currently", DateTime.Now},
				{"warehouse_id", warehouse.Id},
				{"creation_date", DateTime.Now},
				{
					"geographic_group_id", GeographicGroupNodes.Where(x => x.Selected)
						.Select(x => x.GeographicGroup.Id)
				}
			};

			return new ReportInfo {
				Identifier = reportId,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false) =>
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
