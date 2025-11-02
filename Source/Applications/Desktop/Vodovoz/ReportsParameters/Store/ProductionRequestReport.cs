using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProductionRequestReport : SingleUoWWidgetBase, IParametersWidget
	{
		private GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; set; }

		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly int _defaultStockRate = 20;

		public ProductionRequestReport(IReportInfoFactory reportInfoFactory, IEmployeeRepository employeeRepository)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		private void Configure()
		{
			buttonRun.Clicked += (sender, args) => OnUpdate();

			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			yentryrefWarehouse.ChangedByUser += YentryrefWarehouseChangedByUser;

			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
			{
				yentryrefWarehouse.Subject = CurrentUserSettings.Settings.DefaultWarehouse;
			}

			dateperiodpickerMaxSales.StartDate = DateTime.Today.AddYears(-1);
			dateperiodpickerMaxSales.EndDate = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
			dateperiodpickerMaxSales.PeriodChangedByUser += DateperiodpickerMaxSalesPeriodChangedByUser;

			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>(
				UoW.GetAll<GeoGroup>().Select(x => new GeographicGroupNode(x)).ToList());
			
			var employeeGeographicGroup = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Subdivision.GetGeographicGroup();
			
			if(employeeGeographicGroup != null)
			{
				var foundGeoGroup =
					GeographicGroupNodes.FirstOrDefault(x => x.GeographicGroup.Id == employeeGeographicGroup.Id);

				if(foundGeoGroup != null)
				{
					foundGeoGroup.Selected = true;
				}
			}
			
			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
					.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing()
					.AddColumn("Район города").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();
			
			ytreeviewGeographicGroup.ItemsDataSource = GeographicGroupNodes;
			ytreeviewGeographicGroup.HeadersVisible = false;

			yspinStockRate.Digits = 0;
			yspinStockRate.SetRange(0, int.MaxValue);
			yspinStockRate.Value = _defaultStockRate;

			buttonRun.Sensitive = CanRun();
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
			var warehouse = yentryrefWarehouse.Subject as Warehouse;
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpickerMaxSales.StartDateOrNull },
				{ "end_date", dateperiodpickerMaxSales.EndDateOrNull?.AddHours(23).AddMinutes(59).AddSeconds(59) },
				{ "today", DateTime.Today },
				{ "currently", DateTime.Now },
				{ "warehouse_id", warehouse?.Id ?? -1 },
				{ "creation_date", DateTime.Now },
				{ "stock_rate", (int)yspinStockRate.Value },
				{
					"geographic_group_id", GeographicGroupNodes.Where(x => x.Selected)
						.Select(x => x.GeographicGroup.Id)
				}
			};

			var reportInfo = _reportInfoFactory.Create("Store.ProductionRequestReport", Title, parameters);
			return reportInfo;
		}

		void OnUpdate(bool hide = false) =>
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
