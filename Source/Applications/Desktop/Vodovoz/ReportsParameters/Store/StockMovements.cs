using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Reports
{
	public partial class StockMovements : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter filter;

		private GenericObservableList<SelectableSortTypeNode> selectableSortTypeNodes = new GenericObservableList<SelectableSortTypeNode>();

		public StockMovements()
		{
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			yentryrefWarehouse.ItemsQuery = new StoreDocumentHelper(new UserSettingsGetter()).GetRestrictedWarehouseQuery();
			filter = new SelectableParametersReportFilter(UoW);
			
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
			{
				yentryrefWarehouse.Subject = CurrentUserSettings.Settings.DefaultWarehouse;
			}
			
			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			   && !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin)
			{
				yentryrefWarehouse.Sensitive = false;
			}

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
					if (filters != null && filters.Any()) {
						foreach (var f in filters) {
							var filterCriterion = f();
							if (filterCriterion != null) {
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
					if (!selectedValues.Any()) {
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) =>
				{
					var query = UoW.Session.QueryOver<ProductGroup>()
						.Where(p => p.Parent == null)
						.And(p => !p.IsArchive);
					
					if (filters != null && filters.Any())
					{
						foreach (var f in filters)
						{
							query.Where(f());
						}
					}
					return query.List();
				},
				x => x.Name,
				x => x.Childs)
			);

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();

			ytreeSortPriority.ColumnsConfig = FluentColumnsConfig<SelectableSortTypeNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("Имя").AddEnumRenderer(x => x.SortType)
				.Finish();

			ytreeSortPriority.HeadersVisible = false;
			ytreeSortPriority.Reorderable = true;

			ytreeSortPriority.ItemsDataSource = selectableSortTypeNodes;

			foreach (SortType enumItem in Enum.GetValues(typeof(SortType)))
            {
				selectableSortTypeNodes.Add(new SelectableSortTypeNode(enumItem));
			}

			RefreshAvailableSortTypes();

			yentryrefWarehouse.Changed += YentryrefWarehouse_Changed;
		}

		private void RefreshAvailableSortTypes()
        {
			var sortTypeNodes = selectableSortTypeNodes.Where(x => x.SortType == Reports.SortType.GroupOfGoods);

			if (yentryrefWarehouse.Subject == null)
			{
				if (sortTypeNodes.Any())
				{
					selectableSortTypeNodes.Remove(sortTypeNodes.First());
				}
				return;
			}

			if (!sortTypeNodes.Any())
			{
				selectableSortTypeNodes.Add(new SelectableSortTypeNode(Reports.SortType.GroupOfGoods));
			}
		}

		private void YentryrefWarehouse_Changed(object sender, EventArgs e) => RefreshAvailableSortTypes();

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
			if (warehouse == null)
				reportId = "Store.StockWaterMovements";
			else if (warehouse.TypeOfUse == WarehouseUsing.Shipment)
				reportId = "Store.StockShipmentMovements";
			else if (warehouse.TypeOfUse == WarehouseUsing.Production)
				reportId = "Store.StockProductionMovements";
			else
				throw new NotImplementedException("Неизвестный тип использования склада.");

			var parameters = new Dictionary<string, object>
			{
				{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
				{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
				{ "warehouse_id", warehouse?.Id ?? -1 },
				{ "creationDate", DateTime.Now },
				{ "sortType", string.Join(", ", selectableSortTypeNodes.Where(x => x.Selected).Select(x => x.SortType.ToString())) }
			};

			foreach (var item in filter.GetParameters()) {
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

	public enum SortType
	{
		[Display(Name = "Тип")]
		Type,
		[Display(Name = "Группа товаров")]
		GroupOfGoods,
		[Display(Name = "По алфавиту")]
		Alphabet,
	}

	public class SelectableSortTypeNode
	{
        public SelectableSortTypeNode(SortType sortType)
        {
			SortType = sortType;
        }

		public bool Selected { get; set; }

		public SortType SortType { get; private set; }

	}
}
