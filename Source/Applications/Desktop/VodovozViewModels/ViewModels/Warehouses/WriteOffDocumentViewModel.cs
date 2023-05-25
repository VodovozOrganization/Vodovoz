using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
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
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Warehouses
{
	public class WriteOffDocumentViewModel : EntityTabViewModelBase<WriteOffDocument>
	{
		private bool _canChangeDocumentType;
		private WriteOffDocumentItem _selectedItem;
		private INomenclatureRepository _nomenclatureRepository;
		private INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private readonly ILifetimeScope _scope;
		private readonly CommonMessages _commonMessages;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly StoreDocumentHelper _storeDocumentHelper;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IEntityExtendedPermissionValidator _extendedPermissionValidator;

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
			StoreDocumentHelper storeDocumentHelper,
			IEntityExtendedPermissionValidator extendedPermissionValidator)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_storeDocumentHelper = storeDocumentHelper ?? throw new ArgumentNullException(nameof(storeDocumentHelper));
			_extendedPermissionValidator =
				extendedPermissionValidator ?? throw new ArgumentNullException(nameof(extendedPermissionValidator));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));

			Init();
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
					OnPropertyChanged(nameof(HasSelectedFine));
					OnPropertyChanged(nameof(AddOrEditFineTitle));
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
		
		public IEntityEntryViewModel ResponsibleEmployeeViewModel { get; private set; }
		public IEntityEntryViewModel WriteOffFromEmployeeViewModel { get; private set; }
		public IEntityEntryViewModel WriteOffFromCarViewModel { get; private set; }

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(WriteOffDocument), "акта выбраковки"))
				{
					Save();
				}

				var reportInfo = new QS.Report.ReportInfo
				{
					Title = $"Акт выбраковки №{Entity.Id} от {Entity.TimeStamp:d}",
					Identifier = "Store.WriteOff",
					Parameters = new Dictionary<string, object>
					{
						{ "writeoff_id", Entity.Id }
					}
				};

				_reportViewOpener.OpenReport(this, reportInfo);
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
				OnPropertyChanged(nameof(AddOrEditFineTitle));
				OnPropertyChanged(nameof(HasSelectedFine));
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

		private void OnItemSaved(object sender, EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(e.Entity);
			OnPropertyChanged(nameof(AddOrEditFineTitle));
			OnPropertyChanged(nameof(HasSelectedFine));
		}

		private INomenclatureRepository NomenclatureRepository =>
			_nomenclatureRepository ?? (_nomenclatureRepository = _scope.Resolve<INomenclatureRepository>());
		
		private INomenclatureInstanceRepository NomenclatureInstanceRepository =>
			_nomenclatureInstanceRepository ?? (_nomenclatureInstanceRepository = _scope.Resolve<INomenclatureInstanceRepository>());
		
		protected override bool BeforeValidation() => Entity.CanEdit;

		protected override bool BeforeSave()
		{
			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			
			if(Entity.LastEditor == null)
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

		private void Init()
		{
			if(Entity.Id == 0)
			{
				Entity.Author = Entity.ResponsibleEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				if(Entity.Author == null)
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
			SetViewModels();
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
				CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;
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

		private void SetViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<WriteOffDocument>(this, Entity, UoW, NavigationManager, _scope);
			
			ResponsibleEmployeeViewModel = builder.ForProperty(x => x.ResponsibleEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			
			WriteOffFromEmployeeViewModel = builder.ForProperty(x => x.WriteOffFromEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			
			WriteOffFromCarViewModel = builder.ForProperty(x => x.WriteOffFromCar)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
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
			OnPropertyChanged(nameof(AddOrEditFineTitle));
			OnPropertyChanged(nameof(HasSelectedFine));
		}

		private void OnFineSaved(object sender, EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(SelectedItem?.Fine);
			OnPropertyChanged(nameof(HasSelectedFine));
		}
		
		private void FireItemsChanged()
		{
			OnPropertyChanged(nameof(CanChangeStorage));
			OnPropertyChanged(nameof(CanDeleteItem));
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
