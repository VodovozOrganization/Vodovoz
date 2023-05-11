using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.ViewModels;
using QS.Project.Domain;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Controllers;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
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
		private const string _userWithoutEmployee =
			"Ваш пользователь не привязан к действующему сотруднику," +
			" вы не можете работать со складскими документами, так как некого указывать в качестве кладовщика.";
		private readonly IEmployeeService _employeeService;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IStockRepository _stockRepository;
		private readonly INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private readonly CommonMessages _commonMessages;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly ILifetimeScope _scope;
		private bool _isBulkAccountingActive;
		private bool _isInstanceAccountingActive;
		private int _activeAccounting;
		private SelectableParametersReportFilter _selectableFilter;
		private InventoryDocumentItem _selectedNomenclatureItem;
		private InstanceInventoryDocumentItem _selectedInstanceItem;
		private DelegateCommand _confirmCommand;
		private DelegateCommand _printCommand;
		private DelegateCommand _fillNomenclatureItemsByStorageCommand;
		private DelegateCommand _addMissingNomenclatureCommand;
		private DelegateCommand _addOrEditNomenclatureItemFineCommand;
		private DelegateCommand _deleteFineFromNomenclatureItemCommand;
		private DelegateCommand _fillFactByAccountingCommand;
		private DelegateCommand _fillNomenclatureInstanceItemsCommand;
		private DelegateCommand _addMissingNomenclatureInstanceCommand;
		private DelegateCommand _addOrEditNomenclatureInstanceItemFineCommand;
		private DelegateCommand _deleteFineFromNomenclatureInstanceItemCommand;

		public InventoryDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IReportViewOpener reportViewOpener,
			IWarehouseRepository warehouseRepository,
			IStockRepository stockRepository,
			INomenclatureInstanceRepository nomenclatureInstanceRepository,
			ILifetimeScope scope)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_nomenclatureInstanceRepository =
				nomenclatureInstanceRepository ?? throw new ArgumentNullException(nameof(nomenclatureInstanceRepository));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			Init();
		}
		
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

		public bool CanEdit => Entity.CanEdit;
		public bool CanShowWarehouseStorage => Entity.InventoryDocumentType == InventoryDocumentType.WarehouseInventory;
		public bool CanShowEmployeeStorage => Entity.InventoryDocumentType == InventoryDocumentType.EmployeeInventory;
		public bool CanShowCarStorage => Entity.InventoryDocumentType == InventoryDocumentType.CarInventory;
		
		public IEntityEntryViewModel InventoryWarehouseViewModel { get; private set; }
		public IEntityEntryViewModel InventoryEmployeeViewModel { get; private set; }
		public IEntityEntryViewModel InventoryCarViewModel { get; private set; }
		public SelectableParameterReportFilterViewModel SelectableFilterViewModel { get; private set; }
		
		public IEnumerable<Nomenclature> _nomenclaturesWithDiscrepancies = new List<Nomenclature>();

		public DelegateCommand ConfirmCommand => _confirmCommand ?? (_confirmCommand = new DelegateCommand(
			() =>
			{
				//TODO проверка на расхождения в экземплярном учете

				if(Entity.InstanceItems.Any(x => !string.IsNullOrWhiteSpace(x.DiscrepancyDescription)))
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Невозможно подтвердить документ\n" +
						"Имеются расхождения в экземплярном учете");
					return;
				}

				Entity.InventoryDocumentStatus = InventoryDocumentStatus.Confirmed;
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

		public bool CanHandleInventoryItems => Entity.Warehouse != null || Entity.Employee != null || Entity.Car != null;

		#region Объемный учет
		
		public string FillNomenclaturesByStorageTitle
		{
			get
			{
				switch(Entity.InventoryDocumentType)
				{
					case InventoryDocumentType.WarehouseInventory:
						return Entity.ObservableNomenclatureItems.Any() ? _updateByWarehouse : _fillByWarehouse;
					case InventoryDocumentType.EmployeeInventory:
						return Entity.ObservableNomenclatureItems.Any() ? _updateByEmployee : _fillByEmployee;
					case InventoryDocumentType.CarInventory:
						return Entity.ObservableNomenclatureItems.Any() ? _updateByCar : _fillByCar;
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
					var nomenclaturesToInclude = new List<int>();
					var nomenclaturesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					var productGroupToInclude = new List<int>();
					var productGroupToExclude = new List<int>();

					foreach(SelectableParameterSet parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case nameof(Nomenclature):
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
							case nameof(NomenclatureCategory):
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
							case nameof(ProductGroup):
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

					if(Entity.ObservableNomenclatureItems.Count == 0)
					{
						Entity.FillNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude,
							nomenclaturesToExclude: nomenclaturesToExclude,
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude,
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude,
							productGroupToInclude: productGroupToInclude,
							productGroupToExclude: productGroupToExclude);
					}
					else
					{
						Entity.UpdateNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude,
							nomenclaturesToExclude: nomenclaturesToExclude,
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude,
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude,
							productGroupToInclude: productGroupToInclude,
							productGroupToExclude: productGroupToExclude);
					}

					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));
		
		public DelegateCommand AddMissingNomenclatureCommand => _addMissingNomenclatureCommand ?? (
			_addMissingNomenclatureCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
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

		public DelegateCommand FillFactByAccountingCommand => _fillFactByAccountingCommand ?? (
			_fillFactByAccountingCommand = new DelegateCommand(
				() =>
				{
					for(var i = 0; i < Entity.ObservableNomenclatureItems.Count; i++)
					{
						if(Entity.ObservableNomenclatureItems[i].AmountInFact != Entity.ObservableNomenclatureItems[i].AmountInDB)
						{
							Entity.ObservableNomenclatureItems[i].AmountInFact = Entity.ObservableNomenclatureItems[i].AmountInDB;
							Entity.ObservableNomenclatureItems.OnPropertyChanged(nameof(Entity.ObservableNomenclatureItems));
						}
					}
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
						return Entity.ObservableInstanceItems.Any() ? _updateByWarehouse : _fillByWarehouse;
					case InventoryDocumentType.EmployeeInventory:
						return Entity.ObservableInstanceItems.Any() ? _updateByEmployee : _fillByEmployee;
					case InventoryDocumentType.CarInventory:
						return Entity.ObservableInstanceItems.Any() ? _updateByCar : _fillByCar;
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
					var nomenclaturesToInclude = new List<int>();
					var nomenclaturesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					var productGroupToInclude = new List<int>();
					var productGroupToExclude = new List<int>();

					foreach(SelectableParameterSet parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case nameof(Nomenclature):
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
							case nameof(NomenclatureCategory):
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
							case nameof(ProductGroup):
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
					
					IList<NomenclatureInstanceRepository.NomenclatureInstanceBalanceNode> instances = null;

					switch(Entity.InventoryDocumentType)
					{
						case InventoryDocumentType.WarehouseInventory:
							instances = _nomenclatureInstanceRepository.GetInventoryInstancesByStorage(
								UoW,
								OperationType.WarehouseInstanceGoodsAccountingOperation,
								Entity.Warehouse.Id,
								nomenclaturesToInclude,
								nomenclaturesToExclude,
								nomenclatureCategoryToInclude,
								nomenclatureCategoryToExclude,
								productGroupToInclude,
								productGroupToExclude);
							break;
						case InventoryDocumentType.EmployeeInventory:
							instances = _nomenclatureInstanceRepository.GetInventoryInstancesByStorage(
								UoW,
								OperationType.EmployeeInstanceGoodsAccountingOperation,
								Entity.Employee.Id,
								nomenclaturesToInclude,
								nomenclaturesToExclude,
								nomenclatureCategoryToInclude,
								nomenclatureCategoryToExclude,
								productGroupToInclude,
								productGroupToExclude);
							break;
						case InventoryDocumentType.CarInventory:
							instances = _nomenclatureInstanceRepository.GetInventoryInstancesByStorage(
								UoW,
								OperationType.CarInstanceGoodsAccountingOperation,
								Entity.Car.Id,
								nomenclaturesToInclude,
								nomenclaturesToExclude,
								nomenclatureCategoryToInclude,
								nomenclatureCategoryToExclude,
								productGroupToInclude,
								productGroupToExclude);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					foreach(var item in instances)
					{
						if(Entity.ObservableInstanceItems.Any(x => x.InventoryNomenclatureInstance.Id == item.InstanceId))
						{
							continue;
						}
						
						var instanceItem = new InstanceInventoryDocumentItem
						{
							Document = Entity,
							InventoryNomenclatureInstance = item.InventoryNomenclatureInstance,
							IsMissing = true
						};
						Entity.AddInstanceItem(instanceItem);
					}
					
					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));
		
		public DelegateCommand AddMissingNomenclatureInstanceCommand => _addMissingNomenclatureInstanceCommand ?? (
			_addMissingNomenclatureInstanceCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += OnNomenclatureInstanceSelectResult;
				}
			));

		public DelegateCommand AddOrEditNomenclatureInstanceItemFineCommand => _addOrEditNomenclatureInstanceItemFineCommand ?? (
			_addOrEditNomenclatureInstanceItemFineCommand = new DelegateCommand(
				() =>
				{
					FineViewModel fineViewModel;
					if(SelectedInstanceItem.Fine != null)
					{
						fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(SelectedInstanceItem.Fine.Id), OpenPageOptions.AsSlave).ViewModel;
						fineViewModel.EntitySaved += OnExistingInstanceItemFineSaved;
					}
					else
					{
						fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave).ViewModel;
						fineViewModel.Entity.FineReasonString = "Недостача";
						fineViewModel.EntitySaved += OnNewInstanceItemFineSaved;
					}
					fineViewModel.Entity.TotalMoney = SelectedInstanceItem.SumOfDamage;
				}
			));
		
		public DelegateCommand DeleteFineToNomenclatureInstanceItemCommand => _deleteFineFromNomenclatureInstanceItemCommand ?? (
			_deleteFineFromNomenclatureInstanceItemCommand = new DelegateCommand(
				() =>
				{
					UoW.Delete(SelectedInstanceItem.Fine);
					SelectedInstanceItem.Fine = null;
				}
			));

		#endregion
		
		public string GetNomenclatureName(Nomenclature nomenclature)
		{
			return _nomenclaturesWithDiscrepancies.Any(x => x.Id == nomenclature.Id) ? $"<b>{nomenclature.Name}</b>" : nomenclature.Name;
		}
		
		protected override bool BeforeValidation() => CanEdit;

		protected override bool BeforeSave()
		{
			Entity.LastEditor = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			Entity.LastEditedTime = DateTime.Now;
			
			if(Entity.LastEditor == null)
			{
				ShowErrorMessage(_userWithoutEmployee);
				return false;
			}

			Entity.UpdateOperations(UoW);
			return true;
		}

		private void Init()
		{
			var documentHelper = _scope.Resolve<StoreDocumentHelper>();

			if(Entity.Id == 0)
			{
				Entity.Author = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			
				if(Entity.Author == null)
				{
					ShowErrorMessage(_userWithoutEmployee);
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

			if(Entity.SortedByNomenclatureName)
			{
				SortDocumentItems();
			}
		}
		
		private void SetPermissions(StoreDocumentHelper documentHelper)
		{
			var extendedPermissionValidator = _scope.Resolve<IEntityExtendedPermissionValidator>();
			var canEditDocument = documentHelper.CanEditDocument(WarehousePermissionsType.InventoryEdit, Entity.Warehouse);
			var canEditRetroactively = 
				extendedPermissionValidator.Validate(
					typeof(InventoryDocument), UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			Entity.CanEdit = Entity.TimeStamp.Date == DateTime.Today.Date ? canEditDocument : canEditDocument && canEditRetroactively;
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
		
		private void ConfigureSelectableFilter()
		{
			_selectableFilter = _scope.Resolve<SelectableParametersReportFilter>(new TypedParameter(typeof(IUnitOfWork), UoW));

			var nomenclatureParam = _selectableFilter.CreateParameterSet(
				"Номенклатуры",
				nameof(Nomenclature),
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
				nameof(NomenclatureCategory),
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

			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			_selectableFilter.CreateParameterSet(
				"Группы товаров",
				nameof(ProductGroup),
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) =>
				{
					var query = UoW.Session.QueryOver<ProductGroup>()
						.Where(p => p.Parent == null)
						.And(p => !p.IsArchive);

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
			
			SetStoragePropertiesChangeRelation();
			Entity.PropertyChanged += EntityPropertyChanged;
		}

		private void SetStoragePropertiesChangeRelation()
		{
			SetPropertyChangeRelation(x => x.Warehouse, () => CanHandleInventoryItems);
			SetPropertyChangeRelation(x => x.Employee, () => CanHandleInventoryItems);
			SetPropertyChangeRelation(x => x.Car, () => CanHandleInventoryItems);
		}
		
		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.SortedByNomenclatureName))
			{
				SortDocumentItems();
			}
		}

		private void SortDocumentItems()
		{
			Entity.SortItems(Entity.SortedByNomenclatureName);
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
				if(Entity.ObservableNomenclatureItems.Any(x => x.Nomenclature.Id == node.Id))
				{
					continue;
				}

				var nomenclature = UoW.GetById<Nomenclature>(node.Id);
				Entity.AddNomenclatureItem(nomenclature, 0, 0);
			}
		}

		private void OnNomenclatureInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.GetSelectedObjects<InventoryInstancesJournalNode>();
			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var node in selectedNodes)
			{
				if(Entity.InstanceItems.Any(x => x.InventoryNomenclatureInstance.Id == node.Id))
				{
					continue;
				}
				
				var instance = UoW.GetById<InventoryNomenclatureInstance>(node.Id);
				var instanceItem = new InstanceInventoryDocumentItem
				{
					Document = Entity,
					InventoryNomenclatureInstance = instance,
					IsMissing = true,
					CanChangeIsMissing = false
				};
				
				Entity.AddInstanceItem(instanceItem);
			}
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
		
		private void OnNewInstanceItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedInstanceItem.Fine = e.Entity as Fine;
		}

		private void OnExistingInstanceItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = SelectedInstanceItem.Fine.Id;
			UoW.Session.Evict(SelectedInstanceItem.Fine);
			SelectedInstanceItem.Fine = UoW.GetById<Fine>(id);
		}
	}
}
