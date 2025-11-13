using Autofac;
using ClosedXML.Report.Utils;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.PermissionExtensions;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Warehouses
{
	public class WriteOffDocumentViewModel : EntityTabViewModelBase<WriteOffDocument>
	{
		private const string DefectActTitle = "Акт списания";
		private const string WriteOffActTitle = "Акт выбраковки";

		private bool _canChangeDocumentType;
		private WriteOffDocumentItem _selectedItem;
		private INomenclatureRepository _nomenclatureRepository;
		private INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private readonly ILifetimeScope _scope;
		private readonly CommonMessages _commonMessages;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly StoreDocumentHelper _storeDocumentHelper;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IEntityExtendedPermissionValidator _extendedPermissionValidator;
		private readonly IReportInfoFactory _reportInfoFactory;
		private DelegateCommand _printCommand;
		private DelegateCommand _addNomenclatureCommand;
		private DelegateCommand _addOrEditFineCommand;
		private DelegateCommand _addInventoryInstanceCommand;
		private DelegateCommand _deleteItemCommand;
		private DelegateCommand _deleteFineCommand;
		private DelegateCommand _editSelectedItemCommand;

		public WriteOffDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IReportViewOpener reportViewOpener,
			ILifetimeScope scope,
			CommonMessages commonMessages,
			IEmployeeRepository employeeRepository,
			IInteractiveService interactiveService,
			StoreDocumentHelper storeDocumentHelper,
			IEntityExtendedPermissionValidator extendedPermissionValidator,
			IReportInfoFactory reportInfoFactory,
			ViewModelEEVMBuilder<Employee> responsibleEmployeeViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> writeOffEmployeeViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Car> carViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Warehouse> warehouseViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(responsibleEmployeeViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(responsibleEmployeeViewModelEEVMBuilder));
			}

			if(writeOffEmployeeViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(writeOffEmployeeViewModelEEVMBuilder));
			}

			if(carViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(carViewModelEEVMBuilder));
			}

			if(warehouseViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(warehouseViewModelEEVMBuilder));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_storeDocumentHelper = storeDocumentHelper ?? throw new ArgumentNullException(nameof(storeDocumentHelper));
			_extendedPermissionValidator =
				extendedPermissionValidator ?? throw new ArgumentNullException(nameof(extendedPermissionValidator));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));

			Initialize();

			ResponsibleEmployeeViewModel = responsibleEmployeeViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.ResponsibleEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();

			WriteOffFromEmployeeViewModel = writeOffEmployeeViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.WriteOffFromEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();

			WriteOffFromCarViewModel = carViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.WriteOffFromCar)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();

			WarehouseViewModel = warehouseViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.WriteOffFromWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}

		public bool CanChangeDocumentType
		{
			get => _canChangeDocumentType;
			set => SetField(ref _canChangeDocumentType, value);
		}
		
		public WriteOffDocumentItem SelectedItem
		{
			get => _selectedItem;
			set
			{
				if(SetField(ref _selectedItem, value))
				{
					OnPropertyChanged(nameof(CanDeleteItem));
					FireFineChanged();
				};
			}
		}

		public bool CanEdit => Entity.CanEdit;
		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; private set; }
		public bool HasAccessToEmployeeStorages { get; private set; }
		public bool HasAccessToCarStorages { get; private set; }
		public bool CanChangeStorage => CanEdit && !Entity.ObservableItems.Any();
		public bool CanShowWarehouseStorage => Entity.WriteOffType == WriteOffType.Warehouse;
		public bool CanShowEmployeeStorage => Entity.WriteOffType == WriteOffType.Employee;
		public bool CanShowCarStorage => Entity.WriteOffType == WriteOffType.Car;
		public bool CanDeleteItem => SelectedItem != null;
		public bool HasSelectedFine => SelectedItem?.Fine != null;
		public string AddOrEditFineTitle => HasSelectedFine ? "Изменить штраф" : "Добавить штраф";
		public bool CanChangeItems =>
			CanEdit
			&& (Entity.WriteOffFromWarehouse != null
			    || Entity.WriteOffFromEmployee != null
			    || Entity.WriteOffFromCar != null);
		public IEnumerable<Warehouse> Warehouses { get; private set; }
		public IList<CullingCategory> CullingCategories { get; private set; }
		
		public IEntityEntryViewModel ResponsibleEmployeeViewModel { get; }
		public IEntityEntryViewModel WriteOffFromEmployeeViewModel { get; }
		public IEntityEntryViewModel WriteOffFromCarViewModel { get; }
		public IEntityEntryViewModel WarehouseViewModel { get; }

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(WriteOffDocument), "акта выбраковки"))
				{
					if(!Save())
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, "Не удалось сохранить документ, попробуйте еще раз");
						return;
					}
				}

				string selectedActType = WriteOffActTitle;

				if(Entity.WriteOffType == WriteOffType.Car)
				{
					if(Entity.Items.Any(x => x.CullingCategory == null))
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning,
							"Поле \"Причина выбраковки\" не должно быть пустым",
							"Предупреждение");
						return;
					}

					selectedActType = _interactiveService.Question(new string[]
					{
						DefectActTitle,
						WriteOffActTitle
					}, "Выберите, какой документ вы хотите распечатать:", "Тип акта списания");

					if(selectedActType.IsNullOrWhiteSpace())
					{
						return;
					}
				}

				PrintReport(selectedActType);
			}
			));

		public DelegateCommand AddNomenclatureCommand => _addNomenclatureCommand ?? (_addNomenclatureCommand = new DelegateCommand(
			() =>
			{
				var filterParams = GetNomenclatureStockBalanceFilterByStorage();
				
				var page = NavigationManager
					.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(
					this, filterParams, OpenPageOptions.AsSlave);

				page.ViewModel.SelectionMode = JournalSelectionMode.Single;
				page.ViewModel.OnEntitySelectedResult += (s, ea) =>
				{
					var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
					
					if(selectedNode == null)
					{
						return;
					}
					
					if(Entity.Items.Any(x => x.AccountingType == AccountingType.Bulk && x.Nomenclature.Id == selectedNode.Id))
					{
						return;
					}
					
					var nomenclature = NomenclatureRepository.GetNomenclature(UoW, selectedNode.Id);

					Entity.AddItem(nomenclature, 0, selectedNode.StockAmount);
					FireItemsChanged();
				};
			}));

		public DelegateCommand AddInventoryInstanceCommand =>
			_addInventoryInstanceCommand ?? (_addInventoryInstanceCommand = new DelegateCommand(
				() =>
				{
					var filterParams = GetInstancesStockBalanceFilterByStorage();
					var page = NavigationManager.OpenViewModel<
						InventoryInstancesStockBalanceJournalViewModel, Action<InventoryInstancesStockBalanceJournalFilterViewModel>>(
						this, filterParams, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += OnInventoryInstanceSelectResult;
				}));

		public DelegateCommand DeleteItemCommand => _deleteItemCommand ?? (_deleteItemCommand = new DelegateCommand(
			() =>
			{
				Entity.DeleteItem(SelectedItem);
				FireItemsChanged();
			}));
		
		public DelegateCommand AddOrEditFineCommand => _addOrEditFineCommand ?? (_addOrEditFineCommand = new DelegateCommand(
			() =>
			{
				FineViewModel fineViewModel;
				if(SelectedItem.Fine != null)
				{
					fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(SelectedItem.Fine.Id), OpenPageOptions.AsSlave).ViewModel;
					fineViewModel.EntitySaved += OnFineSaved;
				}
				else
				{
					fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave).ViewModel;
					fineViewModel.Entity.FineReasonString = "Недостача";
					fineViewModel.EntitySaved += OnNewFineSaved;
				}
				fineViewModel.Entity.TotalMoney = SelectedItem.SumOfDamage;
			}));
		
		public DelegateCommand DeleteFineCommand => _deleteFineCommand ?? (_deleteFineCommand = new DelegateCommand(
			() =>
			{
				UoW.Delete(SelectedItem.Fine);
				SelectedItem.Fine = null;
				FireFineChanged();
			}));
		
		public DelegateCommand EditSelectedItemCommand => _editSelectedItemCommand ?? (_editSelectedItemCommand = new DelegateCommand(
			() =>
			{
				if(SelectedItem is null)
				{
					return;
				}

				if(SelectedItem is InstanceWriteOffDocumentItem instanceItem)
				{
					var page = NavigationManager.OpenViewModel<InventoryInstanceViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(instanceItem.InventoryNomenclatureInstance.Id), OpenPageOptions.AsSlave);
					page.ViewModel.EntitySaved += OnItemSaved;
				}
				else if(SelectedItem is BulkWriteOffDocumentItem bulkItem)
				{
					var page = NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(bulkItem.Nomenclature.Id), OpenPageOptions.AsSlave);
					page.ViewModel.EntitySaved += OnItemSaved;
				}
			}));

		private void PrintReport(string actType)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"{actType} №{Entity.Id} от {Entity.TimeStamp:d}";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "writeoff_id", Entity.Id }
			};

			switch(actType)
			{
				case DefectActTitle:
					reportInfo.Identifier = "Store.DefectReport";
					break;
				case WriteOffActTitle:
					reportInfo.Identifier = "Store.WriteOff";
					break;
			}

			_reportViewOpener.OpenReport(this, reportInfo);
		}
		private void OnItemSaved(object sender, EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(e.Entity);
			FireFineChanged();
		}

		private INomenclatureRepository NomenclatureRepository =>
			_nomenclatureRepository ?? (_nomenclatureRepository = _scope.Resolve<INomenclatureRepository>());
		
		private INomenclatureInstanceRepository NomenclatureInstanceRepository =>
			_nomenclatureInstanceRepository ?? (_nomenclatureInstanceRepository = _scope.Resolve<INomenclatureInstanceRepository>());

		protected override bool BeforeValidation() => Entity.CanEdit;

		protected override bool BeforeSave()
		{
			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			Entity.LastEditedTime = DateTime.Now;
			
			if(Entity.LastEditorId == null)
			{
				ShowErrorMessage(
					"Ваш пользователь не привязан к действующему сотруднику," +
					" вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			foreach(var item in Entity.Items)
			{
				if(item is InstanceWriteOffDocumentItem instanceItem)
				{
					instanceItem.InventoryNomenclatureInstance.IsArchive = true;
				}
			}
			
			return true;
		}

		private void Initialize()
		{
			if(Entity.Id == 0)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.ResponsibleEmployee = currentEmployee;
				Entity.AuthorId = currentEmployee?.Id;
				if(Entity.AuthorId == null)
				{
					ShowErrorMessage(
						"Ваш пользователь не привязан к действующему сотруднику," +
					    " вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
					FailInitialize = true;
					return;
				}

				Entity.WriteOffFromWarehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.WriteoffEdit);
			}
			else
			{
				CanChangeDocumentType = false;
			}
			
			if(CheckAllStoragesPermissions())
			{
				FailInitialize = true;
				return;
			}
			
			SetPermissions();
			SetOtherProperties();
			SetPropertyChangeRelations();
		}

		private bool CheckAllStoragesPermissions()
		{
			if(Entity.WriteOffType == WriteOffType.Warehouse)
			{
				return _storeDocumentHelper.CheckAllPermissions(
					UoW.IsNew, WarehousePermissionsType.WriteoffEdit, Entity.WriteOffFromWarehouse);
			}

			return false;
		}
		
		private void SetPermissions()
		{
			UserHasOnlyAccessToWarehouseAndComplaints =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !CommonServices.UserService.GetCurrentUser().IsAdmin;
			HasAccessToEmployeeStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_employee_storage_in_warehouse_documents");
			HasAccessToCarStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_car_storage_in_warehouse_documents");
			
			var canEditDocument = CheckPermissionsStorages();
			var canEditRetroactively = _extendedPermissionValidator.Validate(
				typeof(WriteOffDocument), UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			
			Entity.CanEdit = Entity.TimeStamp.Date == DateTime.Today.Date ? canEditDocument : canEditDocument && canEditRetroactively;
		}

		private bool CheckPermissionsStorages()
		{
			return _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.WriteoffEdit, Entity.WriteOffFromWarehouse);
		}
		
		private void SetOtherProperties()
		{
			Warehouses = _storeDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.WriteoffEdit);
			CullingCategories = _scope.Resolve<ICullingCategoryRepository>().GetAllCullingCategories(UoW);
		}
		
		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.WriteOffType,
				() => CanShowWarehouseStorage,
				() => CanShowEmployeeStorage,
				() => CanShowCarStorage);
			
			SetPropertyChangeRelation(
				x => x.WriteOffFromWarehouse,
				() => CanChangeItems);
			SetPropertyChangeRelation(
				x => x.WriteOffFromEmployee,
				() => CanChangeItems);
			SetPropertyChangeRelation(
				x => x.WriteOffFromCar,
				() => CanChangeItems);
		}
		
		private void OnNewFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedItem.Fine = e.GetEntity<Fine>();
			FireFineChanged();
		}

		private void OnFineSaved(object sender, EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(SelectedItem?.Fine);
			FireFineChanged();
		}
		
		private void FireItemsChanged()
		{
			OnPropertyChanged(nameof(CanChangeStorage));
			OnPropertyChanged(nameof(CanDeleteItem));
			FireFineChanged();
		}
		
		private void FireFineChanged()
		{
			OnPropertyChanged(nameof(AddOrEditFineTitle));
			OnPropertyChanged(nameof(HasSelectedFine));
		}
		
		private void OnInventoryInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedItem = e.GetSelectedObjects<InventoryInstancesStockJournalNode>().FirstOrDefault();

			if(selectedItem is null)
			{
				return;
			}

			if(Entity.Items.OfType<InstanceWriteOffDocumentItem>().Any(
					x => x.InventoryNomenclatureInstance.Id == selectedItem.Id))
			{
				return;
			}

			var inventoryInstance = NomenclatureInstanceRepository.GetInventoryNomenclatureInstance(UoW, selectedItem.Id);

			Entity.AddItem(inventoryInstance, 1, selectedItem.Balance);
			FireItemsChanged();
		}
		
		private Action<NomenclatureStockFilterViewModel> GetNomenclatureStockBalanceFilterByStorage()
		{
			Action<NomenclatureStockFilterViewModel> filterParams = null;
			
			switch(Entity.WriteOffType)
			{
				case WriteOffType.Warehouse:
					return filterParams = f =>
					{
						f.RestrictWarehouse = Entity.WriteOffFromWarehouse;
					};
				case WriteOffType.Employee:
					return filterParams = f =>
					{
						f.RestrictEmployeeStorage = Entity.WriteOffFromEmployee;
					};
				case WriteOffType.Car:
					return filterParams = f =>
					{
						f.RestrictCarStorage = Entity.WriteOffFromCar;
					};
				default:
					return filterParams;
			}
		}
		
		private Action<InventoryInstancesStockBalanceJournalFilterViewModel> GetInstancesStockBalanceFilterByStorage()
		{
			Action<InventoryInstancesStockBalanceJournalFilterViewModel> filterParams = null;
			
			switch(Entity.WriteOffType)
			{
				case WriteOffType.Warehouse:
					return filterParams = f =>
					{
						f.IsShow = true;
						f.RestrictedStorageType = StorageType.Warehouse;
						f.RestrictedWarehouse = Entity.WriteOffFromWarehouse;
					};
				case WriteOffType.Employee:
					return filterParams = f =>
					{
						f.IsShow = true;
						f.RestrictedStorageType = StorageType.Employee;
						f.RestrictedEmployeeStorage = Entity.WriteOffFromEmployee;
					};
				case WriteOffType.Car:
					return filterParams = f =>
					{
						f.IsShow = true;
						f.RestrictedStorageType = StorageType.Car;
						f.RestrictedCarStorage = Entity.WriteOffFromCar;
					};
				default:
					return filterParams;
			}
		}
	}
}
