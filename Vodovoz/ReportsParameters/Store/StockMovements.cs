using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Reports
{
	public partial class StockMovements : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter filter;

		public StockMovements()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			filter = new SelectableParametersReportFilter(UoW);
			if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				yentryrefWarehouse.Subject =  CurrentUserSettings.Settings.DefaultWarehouse;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodpicker1.StartDate = dateperiodpicker1.EndDate = DateTime.Today;

			var nomenclatureTypeParam = filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			var nomenclatureParam = filter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							var filterCriterion = f();
							if(filterCriterion != null) {
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() => {
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					if(!selectedValues.Any()) {
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public string Title => "Складские движения";

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
			string reportId;
			var warehouse = yentryrefWarehouse.Subject as Warehouse;
			if(warehouse == null)
				reportId = "Store.StockWaterMovements";
			else if(warehouse.TypeOfUse == WarehouseUsing.Shipment)
				reportId = "Store.StockShipmentMovements";
			else if(warehouse.TypeOfUse == WarehouseUsing.Production)
				reportId = "Store.StockProductionMovements";
			else
				throw new NotImplementedException("Неизвестный тип использования склада.");

			var parameters = new Dictionary<string, object>
			{
				{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
				{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
				{ "warehouse_id", warehouse?.Id ?? -1},
				{ "creationDate", DateTime.Now}
			};

			foreach(var item in filter.GetParameters()) {
				parameters.Add(item.Key, item.Value);
			}

			return new ReportInfo
			{
				Identifier = reportId,
				Parameters = parameters
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

