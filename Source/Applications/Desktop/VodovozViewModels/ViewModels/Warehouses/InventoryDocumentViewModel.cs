using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.ViewModels;
using Vodovoz.Domain.Documents;
using QS.Project.Domain;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.ViewModels.Warehouses
{
	public class InventoryDocumentViewModel : EntityTabViewModelBase<InventoryDocument>
	{
		private const string _updateByWarehouse = "Обновить по складу";
		private const string _updateByEmployee = "Обновить по сотруднику";
		private const string _updateByCar = "Обновить по автомобилю";
		private const string _fillByWarehouse = "Заполнить по складу";
		private const string _fillByEmployee = "Заполнить по сотруднику";
		private const string _fillByCar = "Заполнить по автомобилю";
		private readonly ILifetimeScope _scope;
		private bool _isBulkAccountingActive;
		private bool _isInstanceAccountingActive;
		private int _activeAccounting;
		private IEmployeeService _employeeService;
		private IWarehouseRepository _warehouseRepository;
		private IStockRepository _stockRepository;
		private CommonMessages _commonMessages;
		private IReportViewOpener _reportViewOpener;
		private SelectableParametersReportFilter _selectableFilter;
		private InventoryDocumentItem _selectedNomenclatureItem;
		private InstanceInventoryDocumentItem _selectedInstanceItem;
		private DelegateCommand _acceptCommand;
		private DelegateCommand _printCommand;
		private DelegateCommand _fillNomenclatureItemsByStorageCommand;
		private DelegateCommand _addMissingNomenclatureCommand;
		private DelegateCommand _addOrEditNomenclatureItemFineCommand;
		private DelegateCommand _deleteFineFromNomenclatureItemCommand;
		private DelegateCommand _fillNomenclatureInstanceItemsCommand;
		private DelegateCommand _addMissingNomenclatureInstanceCommand;
		private DelegateCommand _addOrEditNomenclatureInstanceItemFineCommand;
		private DelegateCommand _deleteFineFromNomenclatureInstanceItemCommand;

		public InventoryDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope) : base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}
			
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			Init();
		}
		
		public bool CanEditDocument { get; private set; }
		public bool IsBulkAccountingActive
		{
			get => _isBulkAccountingActive;
			set
			{
				if(SetField(ref _isBulkAccountingActive, value))
				{
					if(value)
					{
						ActiveAccounting = 0;
					}
				}
			}
		}

		public bool IsInstanceAccountingActive
		{
			get => _isInstanceAccountingActive;
			set
			{
				if(SetField(ref _isInstanceAccountingActive, value))
				{
					if(value)
					{
						ActiveAccounting = 1;
					}
				}
			}
		}

		public int ActiveAccounting
		{
			get => _activeAccounting;
			set => SetField(ref _activeAccounting, value);
		}

		public InventoryDocumentItem SelectedNomenclatureInstanceItem
		{
			get => _selectedNomenclatureItem;
			set
			{
				if(SetField(ref _selectedNomenclatureItem, value))
				{
					OnPropertyChanged(nameof(SelectedNomenclatureInstanceItemHasFine));
				}
			}
		}
		
		public bool CanShowWarehouseStorage => Entity.InventoryDocumentType == InventoryDocumentType.WarehouseInventory;
		public bool CanShowEmployeeStorage => Entity.InventoryDocumentType == InventoryDocumentType.EmployeeInventory;
		public bool CanShowCarStorage => Entity.InventoryDocumentType == InventoryDocumentType.CarInventory;
		public bool SelectedNomenclatureInstanceItemHasFine => SelectedNomenclatureInstanceItem?.Fine != null;
		
		public IEntityEntryViewModel InventoryWarehouseViewModel { get; private set; }
		public IEntityEntryViewModel InventoryEmployeeViewModel { get; private set; }
		public IEntityEntryViewModel InventoryCarViewModel { get; private set; }
		public SelectableParameterReportFilterViewModel SelectableFilterViewModel { get; private set; }
		
		public IEnumerable<Nomenclature> _nomenclaturesWithDiscrepancies = new List<Nomenclature>();

		public DelegateCommand AcceptCommand => _acceptCommand ?? (_acceptCommand = new DelegateCommand(
			() =>
			{
				
			}
		));
		
		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(InventoryDocument), "акта инвентаризации"))
				{
					Save();
				}

				var reportInfo = new QS.Report.ReportInfo {
					Title = $"Акт инвентаризации №{Entity.Id} от {Entity.TimeStamp:d}",
					Identifier = "Store.InventoryDoc",
					Parameters = new Dictionary<string, object>
					{
						{ "inventory_id",  Entity.Id }
					}
				};

				_reportViewOpener.OpenReport(this, reportInfo);
			}
			));

		#region Объемный учет
		
		public string FillNomenclaturesByStorageTitle
		{
			get
			{
				switch(Entity.InventoryDocumentType)
				{
					case InventoryDocumentType.WarehouseInventory:
						return Entity.NomenclatureItems.Any() ? _updateByWarehouse : _fillByWarehouse;
					case InventoryDocumentType.EmployeeInventory:
						return Entity.NomenclatureItems.Any() ? _updateByEmployee : _fillByEmployee;
					case InventoryDocumentType.CarInventory:
						return Entity.NomenclatureItems.Any() ? _updateByCar : _fillByCar;
					default:
						throw new InvalidOperationException("Выбран неверный тип документа");
				}
			}
		}
		
		public InventoryDocumentItem SelectedNomenclatureItem
		{
			get => _selectedNomenclatureItem;
			set
			{
				if(SetField(ref _selectedNomenclatureItem, value))
				{
					OnPropertyChanged(nameof(SelectedNomenclatureItemHasFine));
					OnPropertyChanged(nameof(HasSelectedNomenclatureItem));
				}
			}
		}
		
		public string AddOrEditNomenclatureItemFineTitle => SelectedNomenclatureItemHasFine ? "Изменить штраф" : "Добавить штраф";
		public bool SelectedNomenclatureItemHasFine => SelectedNomenclatureItem?.Fine != null;
		public bool HasSelectedNomenclatureItem => SelectedNomenclatureItem != null;

		public DelegateCommand FillNomenclatureItemsByStorageCommand => _fillNomenclatureItemsByStorageCommand ?? (
			_fillNomenclatureItemsByStorageCommand = new DelegateCommand(
				() =>
				{
					// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
					List<int> nomenclaturesToInclude = new List<int>();
					List<int> nomenclaturesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					List<int> productGroupToInclude = new List<int>();
					List<int> productGroupToExclude = new List<int>();

					foreach(SelectableParameterSet parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case "nomenclature":
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclaturesToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclaturesToExclude.Add(value.EntityId);
									}
								}
								break;
							case "nomenclature_type":
								if(parameterSet.FilterType == SelectableFilterType.Include) {
									foreach(var value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								} else {
									foreach(var value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclatureCategoryToExclude.Add((NomenclatureCategory)value.Value);
									}
								}
								break;
							case "product_group":
								if(parameterSet.FilterType == SelectableFilterType.Include) {
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										productGroupToInclude.Add(value.EntityId);
									}
								} else {
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										productGroupToExclude.Add(value.EntityId);
									}
								}
								break;
						}
					}

					FillDiscrepancies();

					if(Entity.NomenclatureItems.Count == 0)
					{
						Entity.FillNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
							nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
							productGroupToInclude: productGroupToInclude.ToArray(),
							productGroupToExclude: productGroupToExclude.ToArray());
					}
					else
					{
						Entity.UpdateNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
							nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
							productGroupToInclude: productGroupToInclude.ToArray(),
							productGroupToExclude: productGroupToExclude.ToArray());
					}

					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));
		
		public DelegateCommand AddMissingNomenclatureCommand => _addMissingNomenclatureCommand ?? (
			_addMissingNomenclatureCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.OnEntitySelectedResult += OnMissingNomenclatureSelectedResult;
				}
			));
		
		public DelegateCommand AddOrEditNomenclatureItemFineCommand => _addOrEditNomenclatureItemFineCommand ?? (
			_addOrEditNomenclatureItemFineCommand = new DelegateCommand(
				() =>
				{
					FineViewModel fineViewModel;
					if(SelectedNomenclatureItem.Fine != null)
					{
						fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(SelectedNomenclatureItem.Fine.Id), OpenPageOptions.AsSlave).ViewModel;
						fineViewModel.EntitySaved += OnExistingNomenclatureItemFineSaved;
					}
					else
					{
						fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave).ViewModel;
						fineViewModel.Entity.FineReasonString = "Недостача";
						fineViewModel.EntitySaved += OnNewNomenclatureItemFineSaved;
					}
					fineViewModel.Entity.TotalMoney = SelectedNomenclatureItem.SumOfDamage;
				}
			));
		
		public DelegateCommand DeleteFineFromNomenclatureItemCommand => _deleteFineFromNomenclatureItemCommand ?? (
			_deleteFineFromNomenclatureItemCommand = new DelegateCommand(
				() =>
				{
					UoW.Delete(SelectedNomenclatureItem.Fine);
					SelectedNomenclatureItem.Fine = null;
				}
			));

		#endregion
		
		#region Экземплярный учет

		public string FillNomenclatureInstancesByStorageTitle
		{
			get
			{
				switch(Entity.InventoryDocumentType)
				{
					case InventoryDocumentType.WarehouseInventory:
						return Entity.NomenclatureItems.Any() ? _updateByWarehouse : _fillByWarehouse;
					case InventoryDocumentType.EmployeeInventory:
						return Entity.NomenclatureItems.Any() ? _updateByEmployee : _fillByEmployee;
					case InventoryDocumentType.CarInventory:
						return Entity.NomenclatureItems.Any() ? _updateByCar : _fillByCar;
					default:
						throw new InvalidOperationException("Выбран неверный тип документа");
				}
			}
		}
		
		public InstanceInventoryDocumentItem SelectedInstanceItem
		{
			get => _selectedInstanceItem;
			set
			{
				if(SetField(ref _selectedInstanceItem, value))
				{
					OnPropertyChanged(nameof(SelectedInstanceItemHasFine));
					OnPropertyChanged(nameof(HasSelectedInstanceItem));
				}
			}
		}
		
		public string AddOrEditInstanceItemFineTitle => SelectedInstanceItemHasFine ? "Изменить штраф" : "Добавить штраф";
		public bool SelectedInstanceItemHasFine => SelectedInstanceItem?.Fine != null;
		public bool HasSelectedInstanceItem => SelectedInstanceItem != null;
		
		public DelegateCommand FillNomenclatureInstanceItemsCommand => _fillNomenclatureInstanceItemsCommand ?? (
			_fillNomenclatureInstanceItemsCommand = new DelegateCommand(
				() =>
				{
					// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
					List<int> nomenclaturesToInclude = new List<int>();
					List<int> nomenclaturesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					List<int> productGroupToInclude = new List<int>();
					List<int> productGroupToExclude = new List<int>();

					foreach(SelectableParameterSet parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case "nomenclature":
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclaturesToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclaturesToExclude.Add(value.EntityId);
									}
								}
								break;
							case "nomenclature_type":
								if(parameterSet.FilterType == SelectableFilterType.Include) {
									foreach(var value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								} else {
									foreach(var value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclatureCategoryToExclude.Add((NomenclatureCategory)value.Value);
									}
								}
								break;
							case "product_group":
								if(parameterSet.FilterType == SelectableFilterType.Include) {
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										productGroupToInclude.Add(value.EntityId);
									}
								} else {
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										productGroupToExclude.Add(value.EntityId);
									}
								}
								break;
						}
					}

					FillDiscrepancies();

					if(Entity.NomenclatureItems.Count == 0)
					{
						Entity.FillNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
							nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
							productGroupToInclude: productGroupToInclude.ToArray(),
							productGroupToExclude: productGroupToExclude.ToArray());
					}
					else
					{
						Entity.UpdateNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
							nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
							productGroupToInclude: productGroupToInclude.ToArray(),
							productGroupToExclude: productGroupToExclude.ToArray());
					}

					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));
		
		public DelegateCommand AddMissingNomenclatureInstanceCommand => _addMissingNomenclatureInstanceCommand ?? (
			_addMissingNomenclatureInstanceCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.OnSelectResult += OnNomenclatureInstanceSelectResult;
				}
			));

		public DelegateCommand AddOrEditNomenclatureInstanceItemFineCommand => _addOrEditNomenclatureInstanceItemFineCommand ?? (
			_addOrEditNomenclatureInstanceItemFineCommand = new DelegateCommand(
				() =>
				{
					
				}
			));
		
		public DelegateCommand DeleteFineToNomenclatureInstanceItemCommand => _deleteFineFromNomenclatureInstanceItemCommand ?? (
			_deleteFineFromNomenclatureInstanceItemCommand = new DelegateCommand(
				() =>
				{
					
				}
			));

		#endregion
		
		public string GetNomenclatureName(Nomenclature nomenclature)
		{
			return _nomenclaturesWithDiscrepancies.Any(x => x.Id == nomenclature.Id) ? $"<b>{nomenclature.Name}</b>" : nomenclature.Name;
		}
		
		protected override bool BeforeValidation() => Entity.CanEdit;

		protected override bool BeforeSave()
		{
			Entity.LastEditor = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			Entity.LastEditedTime = DateTime.Now;
			
			if(Entity.LastEditor == null)
			{
				ShowErrorMessage("Ваш пользователь не привязан к действующему сотруднику," +
				    " вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);
			return true;
		}

		private void Init()
		{
			ResolveInnerDependencies();
			
			var documentHelper = _scope.Resolve<StoreDocumentHelper>();

			if(Entity.Id == 0)
			{
				Entity.Author = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			
				if(Entity.Author == null)
				{
					ShowErrorMessage(
						"Ваш пользователь не привязан к действующему сотруднику," +
						" вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
					FailInitialize = true;
					return;
				}
				Entity.Warehouse = documentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.InventoryEdit);
			}
			
			//TODO уточнить по поводу этого условия
			if(documentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.InventoryEdit, Entity.Warehouse))
			{
				FailInitialize = true;
				return;
			}
			
			SetPermissions(documentHelper);
			SetStoragesViewModels();
			ConfigureSelectableFilter();
			SetPropertyChangeRelations();
		}
		
		private void SetPermissions(StoreDocumentHelper documentHelper)
		{
			var extendedPermissionValidator = _scope.Resolve<IEntityExtendedPermissionValidator>();
			CanEditDocument = documentHelper.CanEditDocument(WarehousePermissionsType.InventoryEdit, Entity.Warehouse);
			Entity.CanEdit =
				extendedPermissionValidator.Validate(
					typeof(InventoryDocument), UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
		}
		
		private void SetStoragesViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryDocument>(this, Entity, UoW, NavigationManager, _scope);
			
			InventoryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.Finish();
			
			InventoryEmployeeViewModel = builder.ForProperty(x => x.Employee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
			
			InventoryCarViewModel = builder.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
		}
		
		private void ResolveInnerDependencies()
		{
			_employeeService = _scope.Resolve<IEmployeeService>();
			_warehouseRepository = _scope.Resolve<IWarehouseRepository>();
			_stockRepository = _scope.Resolve<IStockRepository>();
			_commonMessages = _scope.Resolve<CommonMessages>();
			_reportViewOpener = _scope.Resolve<IReportViewOpener>();
		}
		
		private void ConfigureSelectableFilter()
		{
			_selectableFilter = _scope.Resolve<SelectableParametersReportFilter>(new TypedParameter(typeof(IUnitOfWork), UoW));

			var nomenclatureParam = _selectableFilter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
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

			var nomenclatureTypeParam = _selectableFilter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() => {
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					if(!selectedValues.Any())
					{
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_selectableFilter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) =>
				{
					var query = UoW.Session.QueryOver<ProductGroup>()
						.Where(p => p.Parent == null);

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}
					return query.List();
				},
				x => x.Name,
				x => x.Childs)
			);

			SelectableFilterViewModel = _scope.Resolve<SelectableParameterReportFilterViewModel>(
				new TypedParameter(typeof(SelectableParametersReportFilter), _selectableFilter));
		}
		
		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.InventoryDocumentType,
				() => CanShowWarehouseStorage,
				() => CanShowEmployeeStorage,
				() => CanShowCarStorage);
			
			SetPropertyChangeRelation(
				x => x.InventoryDocumentType,
				() => FillNomenclaturesByStorageTitle,
				() => FillNomenclatureInstancesByStorageTitle);
		}
		
		private void FillDiscrepancies()
		{
			if(Entity.Warehouse != null && Entity.Warehouse.Id > 0)
			{
				_nomenclaturesWithDiscrepancies = _warehouseRepository.GetDiscrepancyNomenclatures(UoW, Entity.Warehouse.Id);
			}
		}

		private void OnMissingNomenclatureSelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			if(!e.SelectedNodes.Any())
			{
				return;
			}

			foreach(var node in e.SelectedNodes)
			{
				if(Entity.NomenclatureItems.Any(x => x.Nomenclature.Id == node.Id))
				{
					continue;
				}

				var nomenclature = UoW.GetById<Nomenclature>(node.Id);
				Entity.AddNomenclatureItem(nomenclature, 0, 0);
			}
		}

		private void OnNomenclatureInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			throw new NotImplementedException();
		}
		
		private void OnNewNomenclatureItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedNomenclatureItem.Fine = e.Entity as Fine;
		}

		private void OnExistingNomenclatureItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = SelectedNomenclatureItem.Fine.Id;
			UoW.Session.Evict(SelectedNomenclatureItem.Fine);
			SelectedNomenclatureItem.Fine = UoW.GetById<Fine>(id);
		}
	}
}
