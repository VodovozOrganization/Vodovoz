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
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
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
	public class ShiftChangeResidueDocumentViewModel : EntityTabViewModelBase<ShiftChangeWarehouseDocument>
	{
		private const string _updateResidues = "Обновить остатки";
		private const string _fillByWarehouse = "Заполнить по складу";
		private const string _fillByCar = "Заполнить по автомобилю";
		private readonly CommonMessages _commonMessages;
		private readonly StoreDocumentHelper _documentHelper;
		private readonly IStockRepository _stockRepository;
		private readonly INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly ILifetimeScope _scope;
		private bool _isInstanceAccountingActive;
		private bool _isBulkAccountingActive;
		private int _activeAccounting;
		private Employee _currentEmployee;
		private SelectableParametersReportFilter _selectableFilter;
		
		private DelegateCommand _printCommand;
		private DelegateCommand _fillNomenclatureItemsByStorageCommand;
		private DelegateCommand _addMissingNomenclatureCommand;
		private DelegateCommand _fillNomenclatureInstanceItemsCommand;
		private DelegateCommand _addMissingNomenclatureInstanceCommand;

		public ShiftChangeResidueDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			StoreDocumentHelper documentHelper,
			IStockRepository stockRepository,
			INomenclatureInstanceRepository nomenclatureInstanceRepository,
			IReportViewOpener reportViewOpener,
			IEntityExtendedPermissionValidator extendedPermissionValidator,
			ILifetimeScope scope)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_documentHelper = documentHelper ?? throw new ArgumentNullException(nameof(documentHelper));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_nomenclatureInstanceRepository =
				nomenclatureInstanceRepository ?? throw new ArgumentNullException(nameof(nomenclatureInstanceRepository));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			Init(
				employeeService ?? throw new ArgumentNullException(nameof(employeeService)),
				extendedPermissionValidator ?? throw new ArgumentNullException(nameof(extendedPermissionValidator)));
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
		
		public bool CanCreate { get; private set; }
		public bool CanEdit { get; private set; }
		public bool CanSave => CanCreate || CanEdit;
		public bool CanHandleInventoryItems => Entity.Warehouse != null || Entity.Car != null;
		public bool CanShowWarehouseStorage => Entity.ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Warehouse;
		public bool CanShowCarStorage => Entity.ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Car;
		public SelectableParameterReportFilterViewModel SelectableFilterViewModel { get; private set; }
		public IEntityEntryViewModel WarehouseStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel CarStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel EmployeeSenderEntryViewModel { get; private set; }
		public IEntityEntryViewModel EmployeeReceiverEntryViewModel { get; private set; }

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(ShiftChangeWarehouseDocument), "акта передачи остатков"))
				{
					Save();
				}

				var reportInfo = new QS.Report.ReportInfo
				{
					Title = $"Акт передачи остатков №{Entity.Id} от {Entity.TimeStamp:d}",
					Identifier = "Store.ShiftChangeWarehouse",
					Parameters = new Dictionary<string, object>
					{
						{ "document_id",  Entity.Id }
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
				switch(Entity.ShiftChangeResidueDocumentType)
				{
					case ShiftChangeResidueDocumentType.Warehouse:
						return Entity.ObservableNomenclatureItems.Any() ? _updateResidues : _fillByWarehouse;
					case ShiftChangeResidueDocumentType.Car:
						return Entity.ObservableNomenclatureItems.Any() ? _updateResidues : _fillByCar;
					default:
						throw new InvalidOperationException("Выбран неверный тип документа");
				}
			}
		}
		
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

					foreach (var parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName) {
							case nameof(Nomenclature):
								if (parameterSet.FilterType == SelectableFilterType.Include) {
									foreach (SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclaturesToInclude.Add(value.EntityId);
									}
								} else {
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclaturesToExclude.Add(value.EntityId);
									}
								}
								break;
							case nameof(NomenclatureCategory):
								if(parameterSet.FilterType == SelectableFilterType.Include) {
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								} else {
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
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

					if(Entity.NomenclatureItems.Count == 0)
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
						Entity.UpdateItemsFromStock(
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
					page.ViewModel.FilterViewModel.AvailableCategories = Nomenclature.GetCategoriesForGoods();
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnEntitySelectedResult += OnMissingNomenclatureSelectedResult;
				}
			));

		#endregion

		#region Экземплярный учет

		public string FillNomenclatureInstancesByStorageTitle
		{
			get
			{
				switch(Entity.ShiftChangeResidueDocumentType)
				{
					case ShiftChangeResidueDocumentType.Warehouse:
						return Entity.ObservableInstanceItems.Any() ? _updateResidues : _fillByWarehouse;
					case ShiftChangeResidueDocumentType.Car:
						return Entity.ObservableInstanceItems.Any() ? _updateResidues : _fillByCar;
					default:
						throw new InvalidOperationException("Выбран неверный тип документа");
				}
			}
		}
		
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

					foreach (var parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName) {
							case nameof(Nomenclature):
								if (parameterSet.FilterType == SelectableFilterType.Include) {
									foreach (SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclaturesToInclude.Add(value.EntityId);
									}
								} else {
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
										nomenclaturesToExclude.Add(value.EntityId);
									}
								}
								break;
							case nameof(NomenclatureCategory):
								if(parameterSet.FilterType == SelectableFilterType.Include) {
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								} else {
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
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

					switch(Entity.ShiftChangeResidueDocumentType)
					{
						case ShiftChangeResidueDocumentType.Warehouse:
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
						case ShiftChangeResidueDocumentType.Car:
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
						
						Entity.AddInstanceItem(item.InventoryNomenclatureInstance);
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

		#endregion

		protected override bool BeforeValidation() => CanSave;

		protected override bool BeforeSave()
		{
			Entity.LastEditor = _currentEmployee;
			Entity.LastEditedTime = DateTime.Now;
			return true;
		}

		private void Init(IEmployeeService employeeService, IEntityExtendedPermissionValidator extendedPermissionValidator)
		{
			_currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			SetPermissions(extendedPermissionValidator);
			
			if(_currentEmployee is null)
			{
				ShowErrorMessage(
					"Ваш пользователь не привязан к действующему сотруднику, вы не можете работать с данным диалогом," +
					" так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			
			if(!CanCreate && UoW.IsNew)
			{
				FailInitialize = true;
				return;
			}

			if(!CanEdit && !UoW.IsNew)
			{
				ShowWarningMessage("У вас нет прав на изменение этого документа.");
			}

			if(Entity.Id == 0)
			{
				Entity.Author = _currentEmployee;
				Entity.Warehouse = _documentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.ShiftChangeCreate);
			}
			else
			{
				if(CheckNomenclaturesWithoutUnits(out var errorMessage))
				{
					ShowErrorMessage(errorMessage);
					FailInitialize = true;
					return;
				}
			}
			
			ConfigureSelectableFilter();
			SetEntriesViewModels();
			SetPropertyChangeRelations();
		}
		
		private bool CheckNomenclaturesWithoutUnits(out string errorMessage)
		{
			errorMessage = "Не установлены единицы измерения у следующих номенклатур :" + Environment.NewLine;
			var wrongNomenclatures = 0;
			foreach(var item in Entity.NomenclatureItems)
			{
				if(item.Nomenclature.Unit == null)
				{
					errorMessage += $"Номер: {item.Nomenclature.Id}. Название: {item.Nomenclature.Name}{Environment.NewLine}";
					wrongNomenclatures++;
				}
			}
			
			return wrongNomenclatures > 0;
		}

		private void ConfigureSelectableFilter()
		{
			_selectableFilter = _scope.Resolve<SelectableParametersReportFilter>(new TypedParameter(typeof(IUnitOfWork), UoW));

			var nomenclatureParam = _selectableFilter.CreateParameterSet(
				"Номенклатуры",
				nameof(Nomenclature),
				new ParametersFactory(UoW, (filters) =>
				{
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

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Fetch(SelectMode.Fetch, x => x.Childs)
				.List();

			_selectableFilter.CreateParameterSet(
				"Группы товаров",
				nameof(ProductGroup),
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) => 
				{
					var query = UoW.Session.QueryOver<ProductGroup>();
					
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

		private void SetPermissions(IEntityExtendedPermissionValidator extendedPermissionValidator)
		{
			CanCreate =
				Entity.Id == 0 && !_documentHelper.CheckCreateDocument(WarehousePermissionsType.ShiftChangeCreate, Entity.Warehouse);
			
			CanEdit = Entity.Id > 0 && _documentHelper.CanEditDocument(WarehousePermissionsType.ShiftChangeEdit, Entity.Warehouse);

			if(Entity.Id != 0 && Entity.TimeStamp < DateTime.Today)
			{
				CanEdit &= extendedPermissionValidator.Validate(
					typeof(ShiftChangeWarehouseDocument), UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			}
		}

		private void SetEntriesViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<ShiftChangeWarehouseDocument>(this, Entity, UoW, NavigationManager, _scope);
			
			EmployeeSenderEntryViewModel = builder.ForProperty(x => x.Sender)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
			
			EmployeeReceiverEntryViewModel = builder.ForProperty(x => x.Receiver)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
			
			WarehouseStorageEntryViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.Finish();

			CarStorageEntryViewModel = builder.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
		}
		
		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.ShiftChangeResidueDocumentType,
				() => CanShowWarehouseStorage,
				() => CanShowCarStorage);
			
			SetPropertyChangeRelation(
				x => x.ShiftChangeResidueDocumentType,
				() => FillNomenclaturesByStorageTitle,
				() => FillNomenclatureInstancesByStorageTitle);
			
			SetStoragePropertiesChangeRelation();
		}

		private void SetStoragePropertiesChangeRelation()
		{
			SetPropertyChangeRelation(x => x.Warehouse, () => CanHandleInventoryItems);
			SetPropertyChangeRelation(x => x.Car, () => CanHandleInventoryItems);
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
				Entity.AddItem(nomenclature, 0, 0);
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
				Entity.AddInstanceItem(instance, false);
			}
		}
	}
}
