using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
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
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using QS.Report;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;

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
		private readonly IReportInfoFactory _reportInfoFactory;
		private string _instancesDiscrepanciesString;
		private bool _isBulkAccountingActive;
		private bool _isInstanceAccountingActive;
		private int _activeAccounting;
		private SelectableParametersReportFilter _selectableFilter;
		private InventoryDocumentItem _selectedNomenclatureItem;
		private InstanceInventoryDocumentItem _selectedInstanceItem;
		private StoreDocumentHelper _documentHelper;
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
		private Dictionary<int, string> _instancesDiscrepancies = new Dictionary<int, string>();
		private IEnumerable<int> _availableWarehousesIdsForEdit;

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
			ILifetimeScope scope,
			IReportInfoFactory reportInfoFactory
			)
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
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			Init();
		}

		public string DocumentStatus => Entity.InventoryDocumentStatus.GetEnumTitle();

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

		public string InstancesDiscrepanciesString
		{
			get => _instancesDiscrepanciesString;
			set => SetField(ref _instancesDiscrepanciesString, value);
		}

		public bool CanEdit => Entity.CanEdit;
		public bool CanShowWarehouseStorage => Entity.InventoryDocumentType == InventoryDocumentType.WarehouseInventory;
		public bool CanShowEmployeeStorage => Entity.InventoryDocumentType == InventoryDocumentType.EmployeeInventory;
		public bool CanShowCarStorage => Entity.InventoryDocumentType == InventoryDocumentType.CarInventory;
		public bool CanChangeInventoryDocumentType => !Entity.StorageIsNotEmpty();
		public bool HasAccessToEmployeeStorages { get; private set; }
		public bool HasAccessToCarStorages { get; private set; }
		
		public EntityEntryViewModel<Warehouse> InventoryWarehouseViewModel { get; private set; }
		public EntityEntryViewModel<Employee> InventoryEmployeeViewModel { get; private set; }
		public EntityEntryViewModel<Car> InventoryCarViewModel { get; private set; }
		public SelectableParameterReportFilterViewModel SelectableFilterViewModel { get; private set; }
		public IEnumerable<Nomenclature> NomenclaturesWithDiscrepancies { get; private set; } = new List<Nomenclature>();

		public DelegateCommand ConfirmCommand => _confirmCommand ?? (_confirmCommand = new DelegateCommand(
			() =>
			{
				if(!Entity.InstanceItems.Any())
				{
					ConfirmDocument();
					return;
				}
				
				UpdateInstanceDiscrepancies();
				
				if(Entity.InstanceItems.Any(x => x.IsMissing && x.AmountInDB > 0)
					|| _instancesDiscrepancies.Any())
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Невозможно подтвердить документ\n" +
						"Имеются расхождения в экземплярном учете");
					return;
				}

				ConfirmDocument();
			}
		));

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(InventoryDocument), "акта инвентаризации"))
				{
					if(!Save())
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не удалось сохранить документ, попробуйте еще раз");
						return;
					}
				}

				var reportInfo = _reportInfoFactory.Create();
				reportInfo.Title = $"Акт инвентаризации №{Entity.Id} от {Entity.TimeStamp:d}";
				reportInfo.Identifier = "Store.InventoryDoc";
				reportInfo.Parameters = new Dictionary<string, object>
				{
					{ "inventory_id",  Entity.Id },
					{ "sorted_by_nomenclature_name", Entity.SortedByNomenclatureName }
				};

				_reportViewOpener.OpenReport(this, reportInfo);
			}
			));

		public bool CanHandleInventoryItems => CanEdit 
		    && (Entity.Warehouse != null || Entity.Employee != null || Entity.Car != null);

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
					FireNomenclatureItemFineChanged();
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
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(var value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								}
								else
								{
									foreach(var value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclatureCategoryToExclude.Add((NomenclatureCategory)value.Value);
									}
								}
								break;
							case nameof(ProductGroup):
								var selectedProductGroupIds = new List<int>();
								CollectSelectedProductGroupIds(parameterSet.OutputParameters, selectedProductGroupIds);

								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(var groupId in selectedProductGroupIds)
									{
										AddProductGroupWithChildren(groupId, productGroupToInclude);
									}
								}
								else
								{
									foreach(var groupId in selectedProductGroupIds)
									{
										AddProductGroupWithChildren(groupId, productGroupToExclude);
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

		private void CollectSelectedProductGroupIds(IEnumerable<SelectableParameter> parameters, List<int> result)
		{
			foreach(var parameter in parameters)
			{
				if(parameter.Selected && parameter is SelectableEntityParameter<ProductGroup> groupParam)
				{
					result.Add(groupParam.EntityId);
				}

				if(parameter.Children != null && parameter.Children.Any())
				{
					CollectSelectedProductGroupIds(parameter.Children, result);
				}
			}
		}

		private void AddProductGroupWithChildren(int productGroupId, List<int> targetList)
		{
			targetList.Add(productGroupId);

			var productGroup = UoW.GetById<ProductGroup>(productGroupId);
			if(productGroup == null)
			{
				return;
			}

			foreach(var childGroup in productGroup.Childs.Where(x => !x.IsArchive))
			{
				AddProductGroupWithChildren(childGroup.Id, targetList);
			}
		}

		public DelegateCommand AddMissingNomenclatureCommand => _addMissingNomenclatureCommand ?? (
			_addMissingNomenclatureCommand = new DelegateCommand(
				() =>
				{
					NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
						this,
						OpenPageOptions.AsSlave,
						vm =>
						{
							vm.SelectionMode = JournalSelectionMode.Single;
							vm.OnSelectResult += OnMissingNomenclatureSelectedResult;
						});
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
					FireNomenclatureItemFineChanged();
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
					FireInstanceItemFineChanged();
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
					var instancesToInclude = new List<int>();
					var instancesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					var productGroupToInclude = new List<int>();
					var productGroupToExclude = new List<int>();

					foreach(SelectableParameterSet parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case nameof(InventoryNomenclatureInstance):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<InventoryNomenclatureInstance> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										instancesToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<InventoryNomenclatureInstance> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										instancesToExclude.Add(value.EntityId);
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

					Entity.FillInstanceItemsFromStock(
						UoW,
						_nomenclatureInstanceRepository,
						instancesToInclude,
						instancesToExclude,
						nomenclatureCategoryToInclude,
						nomenclatureCategoryToExclude,
						productGroupToInclude,
						productGroupToExclude);
					
					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));

		public DelegateCommand AddMissingNomenclatureInstanceCommand => _addMissingNomenclatureInstanceCommand ?? (
			_addMissingNomenclatureInstanceCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += OnMissingNomenclatureInstanceSelectResult;
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
					FireInstanceItemFineChanged();
				}
			));

		#endregion
		
		public string GetNomenclatureName(Nomenclature nomenclature)
		{
			return NomenclaturesWithDiscrepancies.Any(x => x.Id == nomenclature.Id) ? $"<b>{nomenclature.Name}</b>" : nomenclature.Name;
		}
		
		protected override bool BeforeValidation() => CanEdit;

		protected override bool BeforeSave()
		{
			Entity.LastEditorId = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId)?.Id;
			Entity.LastEditedTime = DateTime.Now;
			
			if(Entity.LastEditorId == null)
			{
				ShowErrorMessage(_userWithoutEmployee);
				return false;
			}

			Entity.UpdateOperations(UoW);
			return true;
		}

		private void Init()
		{
			_documentHelper = _scope.Resolve<StoreDocumentHelper>();
			SetPermissions();

			if(Entity.Id == 0)
			{
				Entity.AuthorId = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId)?.Id;
			
				if(Entity.AuthorId == null)
				{
					ShowErrorMessage(_userWithoutEmployee);
					FailInitialize = true;
					return;
				}
				Entity.Warehouse = _documentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.InventoryEdit);
			}
			
			if(CheckAllPermissionsStorages())
			{
				FailInitialize = true;
				return;
			}
			
			ValidateNomenclatures();
			SetStoragesViewModels();
			ConfigureSelectableFilter();
			SetPropertyChangeRelations();

			if(Entity.InstanceItems.Any() && Entity.InventoryDocumentStatus != InventoryDocumentStatus.Confirmed)
			{
				UpdateInstanceDiscrepancies();
			}

			if(Entity.SortedByNomenclatureName)
			{
				SortDocumentItems();
			}
		}

		private void UpdateInstanceDiscrepancies()
		{
			_instancesDiscrepancies.Clear();
			var storageId = Entity.GetStorageId();
			var storageType = Entity.GetStorageType();
			var currentInstancesIds =
				Entity.ObservableInstanceItems.Select(x => x.InventoryNomenclatureInstance.Id).ToArray();
			
			var instancesOnStorageBalance =
				_nomenclatureInstanceRepository.GetOtherInstancesOnStorageBalance(UoW, storageType, storageId ?? 0, currentInstancesIds);

			foreach(var instanceData in instancesOnStorageBalance)
			{
				_instancesDiscrepancies.Add(
					instanceData.Id,
					$"{instanceData.Name} {instanceData.GetInventoryNumber} числится на этом складе");
			}

			var currentInstancesOnOtherStorages =
				_nomenclatureInstanceRepository.GetCurrentInstancesOnOtherStorages(UoW, storageType, storageId ?? 0, currentInstancesIds);

			if(currentInstancesOnOtherStorages.Any())
			{
				foreach(var groupInstanceData in currentInstancesOnOtherStorages)
				{
					var key = groupInstanceData.Key;
					var instanceData = groupInstanceData.First();
					var storages = string.Join(",", groupInstanceData.Select(x => x.StorageName));
					
					_instancesDiscrepancies.Add(key, $"{instanceData.Name} {instanceData.GetInventoryNumber} числится на: {storages}");
				}
			}

			InstancesDiscrepanciesString = _instancesDiscrepancies.Any() ? string.Join("\n", _instancesDiscrepancies.Values) : string.Empty;
		}

		private void SetPermissions()
		{
			var extendedPermissionValidator = _scope.Resolve<IEntityExtendedPermissionValidator>();
			HasAccessToEmployeeStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_employee_storage_in_warehouse_documents");
			HasAccessToCarStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_car_storage_in_warehouse_documents");
			
			var canEditDocument = CheckPermissionsStorages();
			var canEditRetroactively = 
				extendedPermissionValidator.Validate(
					typeof(InventoryDocument), UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			
			Entity.CanEdit = Entity.TimeStamp.Date == DateTime.Today.Date ? canEditDocument : canEditDocument && canEditRetroactively;
		}
		
		private bool CheckAllPermissionsStorages()
		{
			if(Entity.InventoryDocumentType == InventoryDocumentType.WarehouseInventory)
			{
				return _documentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.InventoryEdit, Entity.Warehouse);
			}
			
			return false;
		}
		
		private bool CheckPermissionsStorages()
		{
			switch(Entity.InventoryDocumentType)
			{
				case InventoryDocumentType.WarehouseInventory:
					return _documentHelper.CanEditDocument(WarehousePermissionsType.InventoryEdit, Entity.Warehouse);
				case InventoryDocumentType.EmployeeInventory:
					return HasAccessToEmployeeStorages;
				case InventoryDocumentType.CarInventory:
					return HasAccessToCarStorages;
				default:
					return false;
			}
		}
		
		private void SetStoragesViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryDocument>(this, Entity, UoW, NavigationManager, _scope);
			
			_availableWarehousesIdsForEdit = _documentHelper.GetRestrictedWarehousesIds(UoW, WarehousePermissionsType.InventoryEdit);
			
			InventoryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(GetWarehouseFilterParams)
				.Finish();
			InventoryWarehouseViewModel.CanViewEntity = false;
			InventoryWarehouseViewModel.BeforeChangeByUser += OnWarehouseBeforeChangeByUser;
			InventoryWarehouseViewModel.ChangedByUser += OnWarehouseChangedByUser;
			
			InventoryEmployeeViewModel = builder.ForProperty(x => x.Employee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			InventoryEmployeeViewModel.CanViewEntity = false;
			InventoryEmployeeViewModel.BeforeChangeByUser += OnEmployeeBeforeChangeByUser;
			InventoryEmployeeViewModel.ChangedByUser += OnEmployeeChangedByUser;
			
			InventoryCarViewModel = builder.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
			InventoryCarViewModel.CanViewEntity = false;
			InventoryCarViewModel.BeforeChangeByUser += OnCarBeforeChangeByUser;
			InventoryCarViewModel.ChangedByUser += OnCarChangedByUser;
		}
		
		private void GetWarehouseFilterParams(WarehouseJournalFilterViewModel filter)
		{
			filter.IncludeWarehouseIds = _availableWarehousesIdsForEdit;
		}

		private void OnWarehouseBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(Entity.StorageIsNotEmpty() && Entity.ItemsNotEmpty())
			{
				if(AskQuestion("При изменении склада табличная часть документа будет очищена. Продолжить?"))
				{
					ClearItems();
				}
				else
				{
					e.CanChange = false;
				}
			}
		}

		private void OnEmployeeBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(Entity.StorageIsNotEmpty() && Entity.ItemsNotEmpty())
			{
				if(AskQuestion("При изменении сотрудника табличная часть документа будет очищена. Продолжить?"))
				{
					ClearItems();
				}
				else
				{
					e.CanChange = false;
				}
			}
		}
		
		private void OnCarBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(Entity.StorageIsNotEmpty() && Entity.ItemsNotEmpty())
			{
				if(AskQuestion("При изменении автомобиля табличная часть документа будет очищена. Продолжить?"))
				{
					ClearItems();
				}
				else
				{
					e.CanChange = false;
				}
			}
		}
		
		private void OnWarehouseChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Warehouse is null)
			{
				ClearItems();
			}
		}
		
		private void OnEmployeeChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Employee is null)
			{
				ClearItems();
			}
		}

		private void OnCarChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Car is null)
			{
				ClearItems();
			}
		}
		
		private void ClearItems()
		{
			Entity.ObservableNomenclatureItems.Clear();
			Entity.ObservableInstanceItems.Clear();
			_instancesDiscrepancies.Clear();
			InstancesDiscrepanciesString = string.Empty;
			OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
			OnPropertyChanged(nameof(FillNomenclatureInstancesByStorageTitle));
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
						.Where(x => !x.IsArchive)
						.And(x => !x.HasInventoryAccounting);
					
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
			
			Nomenclature nomenclatureAlias = null;
			var instancesParam = _selectableFilter.CreateParameterSet("Экземпляры",
				nameof(InventoryNomenclatureInstance),
				new ParametersFactory(UoW, (filters) =>
				{
					InventoryNomenclatureInstance instanceAlias = null;
					SelectableEntityParameter<InventoryNomenclatureInstance> resultAlias = null;

					var query = UoW.Session.QueryOver(() => instanceAlias)
						.JoinAlias(i => i.Nomenclature, () => nomenclatureAlias)
						.Where(i => !i.IsArchive);

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
					
					var customName = CustomProjections.Concat(
						Projections.Property(() => nomenclatureAlias.OfficialName),
						Projections.Constant(" "),
						InventoryNomenclatureInstanceProjections.InventoryNumberProjection());

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(customName).WithAlias(() => resultAlias.EntityTitle)
					).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<InventoryNomenclatureInstance>>());
					return query.List<SelectableParameter>();
				}));
			
			instancesParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedTypes = nomenclatureTypeParam.GetSelectedValues().ToArray();
					if(!selectedTypes.Any())
					{
						return null;
					}
					return Restrictions.On(() => nomenclatureAlias.Category).IsIn(selectedTypes);
				});

			SelectableFilterViewModel = _scope.Resolve<SelectableParameterReportFilterViewModel>(
				new TypedParameter(typeof(SelectableParametersReportFilter), _selectableFilter));
		}
		
		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.InventoryDocumentStatus,
				() => DocumentStatus);
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
			SetPropertyChangeRelation(x => x.Warehouse,
				() => CanHandleInventoryItems,
				() => CanChangeInventoryDocumentType);
			SetPropertyChangeRelation(x => x.Employee,
				() => CanHandleInventoryItems,
				() => CanChangeInventoryDocumentType);
			SetPropertyChangeRelation(x => x.Car,
				() => CanHandleInventoryItems,
				() => CanChangeInventoryDocumentType);
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
				NomenclaturesWithDiscrepancies = _warehouseRepository.GetDiscrepancyNomenclatures(UoW, Entity.Warehouse.Id);
			}
		}

		private void OnMissingNomenclatureSelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.Cast<NomenclatureJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var node in selectedNodes)
			{
				if(Entity.ObservableNomenclatureItems.Any(x => x.Nomenclature.Id == node.Id))
				{
					continue;
				}

				var nomenclature = UoW.GetById<Nomenclature>(node.Id);
				Entity.AddNomenclatureItem(nomenclature, 0, 0);
			}
		}

		private void OnMissingNomenclatureInstanceSelectResult(object sender, JournalSelectedEventArgs e)
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
					AmountInDB = 0,
				};
				
				Entity.AddInstanceItem(instanceItem);
			}
		}
		
		private void OnNewNomenclatureItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedNomenclatureItem.Fine = e.Entity as Fine;
			FireNomenclatureItemFineChanged();
		}

		private void OnExistingNomenclatureItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = SelectedNomenclatureItem.Fine.Id;
			UoW.Session.Evict(SelectedNomenclatureItem.Fine);
			SelectedNomenclatureItem.Fine = UoW.GetById<Fine>(id);
			FireNomenclatureItemFineChanged();
		}
		
		private void OnNewInstanceItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedInstanceItem.Fine = e.Entity as Fine;
			FireInstanceItemFineChanged();
		}

		private void OnExistingInstanceItemFineSaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = SelectedInstanceItem.Fine.Id;
			UoW.Session.Evict(SelectedInstanceItem.Fine);
			SelectedInstanceItem.Fine = UoW.GetById<Fine>(id);
			FireInstanceItemFineChanged();
		}
		
		private void ConfirmDocument()
		{
			Entity.InventoryDocumentStatus = InventoryDocumentStatus.Confirmed;
			SaveAndClose();
		}
		
		private void FireNomenclatureItemFineChanged()
		{
			OnPropertyChanged(nameof(AddOrEditNomenclatureItemFineTitle));
			OnPropertyChanged(nameof(SelectedNomenclatureItemHasFine));
			OnPropertyChanged(nameof(HasSelectedNomenclatureItem));
		}
		
		private void FireInstanceItemFineChanged()
		{
			OnPropertyChanged(nameof(AddOrEditInstanceItemFineTitle));
			OnPropertyChanged(nameof(SelectedInstanceItemHasFine));
			OnPropertyChanged(nameof(HasSelectedInstanceItem));
		}
		
		private void ValidateNomenclatures()
		{
			int wrongNomenclatures = 0;

			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(
				"Не установлены единицы измерения у следующих номенклатур:");

			foreach(var item in Entity.NomenclatureItems)
			{
				if(item.Nomenclature.Unit == null)
				{
					stringBuilder.AppendLine(
						$"Номер: {item.Nomenclature.Id}." +
						$" Название: {item.Nomenclature.Name}");
					wrongNomenclatures++;
				}
			}

			if(wrongNomenclatures > 0)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					stringBuilder.ToString());

				Close(false, CloseSource.Self);
			}
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= EntityPropertyChanged;
			InventoryWarehouseViewModel.BeforeChangeByUser -= OnWarehouseBeforeChangeByUser;
			InventoryWarehouseViewModel.ChangedByUser -= OnWarehouseChangedByUser;
			InventoryEmployeeViewModel.BeforeChangeByUser -= OnEmployeeBeforeChangeByUser;
			InventoryEmployeeViewModel.ChangedByUser -= OnEmployeeChangedByUser;
			InventoryCarViewModel.BeforeChangeByUser -= OnCarBeforeChangeByUser;
			InventoryCarViewModel.ChangedByUser -= OnCarChangedByUser;
			base.Dispose();
		}
	}
}
