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
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports;
using QS.ViewModels.Control.EEVM;
using System.ComponentModel;
using QS.Tdi;
using QS.Navigation;
using Autofac;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Reports
{
	public partial class StockMovements : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly INavigationManager _navigationManager;
		private readonly SelectableParametersReportFilter _filter;
		private readonly GenericObservableList<SelectableSortTypeNode> _selectableSortTypeNodes =
			new GenericObservableList<SelectableSortTypeNode>();
		private Warehouse _warehouse;
		private ITdiTab _parentTab;
		private ILifetimeScope _scope;

		public StockMovements(
			IReportInfoFactory reportInfoFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(UoW);

			ConfigureDlg();
		}

		private IEntityEntryViewModel WarehouseEntryViewModel { get; set; }

		private Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				_warehouse = value;
			}
		}

		public ITdiTab ParentTab
		{
			get => _parentTab;
			set
			{
				_parentTab = value;
				ConfigureWarehouseEntryViewModel();
			}
		}

		private void ConfigureWarehouseEntryViewModel()
		{
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
			{
				Warehouse = CurrentUserSettings.Settings.DefaultWarehouse;
			}

			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
			   && !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin)
			{
				entryWarehouse.Sensitive = false;
			}

			var builder = new LegacyEEVMBuilderFactory<StockMovements>(ParentTab, this, UoW, _navigationManager, _scope);

			WarehouseEntryViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			
			RefreshAvailableSortTypes();
			WarehouseEntryViewModel.Changed += OnWarehouseChanged;
			entryWarehouse.ViewModel = WarehouseEntryViewModel;
		}

		private void ConfigureDlg()
		{
			dateperiodpicker1.StartDate = dateperiodpicker1.EndDate = DateTime.Today;

			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			var nomenclatureParam = _filter.CreateParameterSet(
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

			_filter.CreateParameterSet(
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

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();

			ytreeSortPriority.ColumnsConfig = FluentColumnsConfig<SelectableSortTypeNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("Имя").AddEnumRenderer(x => x.SortType)
				.Finish();

			ytreeSortPriority.HeadersVisible = false;
			ytreeSortPriority.Reorderable = true;

			ytreeSortPriority.ItemsDataSource = _selectableSortTypeNodes;

			foreach (SortType enumItem in Enum.GetValues(typeof(SortType)))
            {
				_selectableSortTypeNodes.Add(new SelectableSortTypeNode(enumItem));
			}
		}

		private void RefreshAvailableSortTypes()
        {
			var sortTypeNodes = _selectableSortTypeNodes.Where(x => x.SortType == Reports.SortType.GroupOfGoods);

			if (Warehouse is null)
			{
				if (sortTypeNodes.Any())
				{
					_selectableSortTypeNodes.Remove(sortTypeNodes.First());
				}
				return;
			}

			if (!sortTypeNodes.Any())
			{
				_selectableSortTypeNodes.Add(new SelectableSortTypeNode(Reports.SortType.GroupOfGoods));
			}
		}

		private void OnWarehouseChanged(object sender, EventArgs e) => RefreshAvailableSortTypes();

        #region IParametersWidget implementation

        public string Title => "Складские движения";

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public event PropertyChangedEventHandler PropertyChanged;

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
			if(Warehouse is null)
			{
				reportId = "Store.StockWaterMovements";
			}
			else if(Warehouse.TypeOfUse == WarehouseUsing.Shipment)
			{
				reportId = "Store.StockShipmentMovements";
			}
			else if(Warehouse.TypeOfUse == WarehouseUsing.Production)
			{
				reportId = "Store.StockProductionMovements";
			}
			else
			{
				throw new NotImplementedException("Неизвестный тип использования склада.");
			}

			var parameters = new Dictionary<string, object>
			{
				{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
				{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
				{ "warehouse_id", Warehouse?.Id ?? -1 },
				{ "creationDate", DateTime.Now },
				{ "sortType", string.Join(", ", _selectableSortTypeNodes.Where(x => x.Selected).Select(x => x.SortType.ToString())) }
			};

			foreach (var item in _filter.GetParameters()) {
				parameters.Add(item.Key, item.Value);
			}

			var reportInfo = _reportInfoFactory.Create(reportId, Title, parameters);
			return reportInfo;
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

		public override void Destroy()
		{
			if(_scope != null)
			{
				_scope.Dispose();
				_scope = null;
			}
			_parentTab = null;

			base.Destroy();
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
