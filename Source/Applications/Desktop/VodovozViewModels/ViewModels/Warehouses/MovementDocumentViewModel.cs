using Autofac;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Journals;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.PermissionExtensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Store;
using VodovozBusiness.CachingRepositories.Employees;

namespace Vodovoz.ViewModels.Warehouses
{
	public class MovementDocumentViewModel : EntityTabViewModelBase<MovementDocument>
	{
		private readonly ILifetimeScope _scope;
		private readonly IEmployeeService _employeeService;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IUserRepository _userRepository;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly IStockRepository _stockRepository;
		private readonly IEmployeeInMemoryNameWithInitialsCacheRepository _employeeInMemoryNameWithInitialsCacheRepository;
		private readonly IWarehousePermissionValidator _warehousePermissionValidator;
		private readonly INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private UserSettings _currentUserSettings;
		private Employee _currentEmployee;
		private bool _canEditRectroactively;
		private bool _canChangeAcceptedMovementDoc;
		private bool _canAcceptMovementDocumentDiscrepancy;
		private bool _canEditStoreMovementDocumentTransporterData;
		private IEntityEntryViewModel _transporterCounterpartyEntryViewModel;

		private IEnumerable<Warehouse> _allowedWarehousesFrom;
		private IEnumerable<Warehouse> _allowedWarehousesTo;

		private DelegateCommand _sendCommand;
		private DelegateCommand _receiveCommand;
		private DelegateCommand _acceptDiscrepancyCommand;
		private DelegateCommand _addItemCommand;
		private DelegateCommand<MovementDocumentItem> _deleteItemCommand;
		private DelegateCommand _fillFromOrdersCommand;
		private DelegateCommand _printCommand;
		private DelegateCommand _addInventoryInstanceCommand;

		public MovementDocumentViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IRDLPreviewOpener rdlPreviewOpener,
			IOrderSelectorFactory orderSelectorFactory,
			IWarehousePermissionValidator warehousePermissionValidator,
			INomenclatureInstanceRepository nomenclatureInstanceRepository,
			IUserRepository userRepository,
			IStockRepository stockRepository,
			ViewModelEEVMBuilder<Warehouse> sourceWarehouseViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Warehouse> targetWarehouseViewModelEEVMBuilder,
			IEmployeeInMemoryNameWithInitialsCacheRepository employeeInMemoryNameWithInitialsCacheRepository,
			ILifetimeScope scope) 
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(sourceWarehouseViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(sourceWarehouseViewModelEEVMBuilder));
			}

			if(targetWarehouseViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(targetWarehouseViewModelEEVMBuilder));
			}

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_warehousePermissionValidator =
				warehousePermissionValidator ?? throw new ArgumentNullException(nameof(warehousePermissionValidator));
			_nomenclatureInstanceRepository = 
				nomenclatureInstanceRepository ?? throw new ArgumentNullException(nameof(nomenclatureInstanceRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_employeeInMemoryNameWithInitialsCacheRepository = employeeInMemoryNameWithInitialsCacheRepository
				?? throw new ArgumentNullException(nameof(employeeInMemoryNameWithInitialsCacheRepository));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			ResolveInnerDependencies();
			SetStoragesViewModels();
			ConfigureEntityChangingRelations();

			if(Entity.Id == 0)
			{
				Entity.DocumentType = MovementDocumentType.Transportation;
				SetDefaultWarehouseFrom();
			}

			SourceWarehouseViewModel = sourceWarehouseViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.FromWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = WarehousesFrom.Select(w => w.Id);
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			TargetWarehouseViewModel = targetWarehouseViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.ToWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = WarehousesTo.Select(w => w.Id);
					filter.IgnorePermissions = true;
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			Entity.PropertyChanged += OnMovementDocumentPropertyChanged;
		}

		private void OnMovementDocumentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.FromWarehouse))
			{
				ReloadAllowedWarehousesTo();
			}

			if(e.PropertyName == nameof(Entity.ToWarehouse))
			{
				ReloadAllowedWarehousesFrom();
			}
		}

		public IEntityEntryViewModel WagonEntryViewModel { get; private set; }
		public IEntityEntryViewModel FromEmployeeStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel ToEmployeeStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel FromCarStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel ToCarStorageEntryViewModel { get; private set; }

		public IEntityEntryViewModel TransporterCounterpartyEntryViewModel
		{ 
			get => _transporterCounterpartyEntryViewModel; 
			set
			{
				if(_transporterCounterpartyEntryViewModel is null)
				{
					_transporterCounterpartyEntryViewModel = value;
				}
			}
		}

		public ILifetimeScope Scope => _scope;

		public bool CanEdit => 
			(Entity.Id == 0 && PermissionResult.CanCreate)
			|| (PermissionResult.CanUpdate &&
			    (Entity.TimeStamp.Date >= DateTime.Today || DateTime.Today <= Entity.TimeStamp.Date.AddDays(4).AddHours(23).AddMinutes(59)))
			|| _canEditRectroactively;
		
		#region Header info

		public string AuthorInfo {
			get {
				if(Entity.AuthorId == null) {
					return null;
				}
				return $"{_employeeInMemoryNameWithInitialsCacheRepository.GetTitleById(Entity.AuthorId.Value)}, {Entity.TimeStamp:dd.MM.yyyy HH:mm}";
			}
		}

		public string LastEditorInfo {
			get {
				if(Entity.LastEditorId == null) {
					return null;
				}
				return $"{_employeeInMemoryNameWithInitialsCacheRepository.GetTitleById(Entity.LastEditorId.Value)}, {Entity.LastEditedTime:dd.MM.yyyy HH:mm}";
			}
		}

		public string SendedInfo {
			get {
				if(Entity.Sender == null || Entity.SendTime == null) {
					return null;
				}
				return $"{Entity.Sender.GetPersonNameWithInitials()}, {Entity.SendTime.Value:dd.MM.yyyy HH:mm}";
			}
		}

		public string ReceiverInfo {
			get {
				if(Entity.Receiver == null || Entity.ReceiveTime == null) {
					return null;
				}
				return $"{Entity.Receiver.GetPersonNameWithInitials()}, {Entity.ReceiveTime.Value:dd.MM.yyyy HH:mm}";
			}
		}

		public string DiscrepancyAccepterInfo {
			get {
				if(Entity.DiscrepancyAccepter == null || Entity.DiscrepancyAcceptTime == null) {
					return null;
				}
				return $"{Entity.DiscrepancyAccepter.GetPersonNameWithInitials()}, {Entity.DiscrepancyAcceptTime.Value:dd.MM.yyyy HH:mm}";
			}
		}

		#endregion Header info

		#region Warehouses

		public IEnumerable<Warehouse> AllowedWarehousesFrom {
			get {
				if(_allowedWarehousesFrom == null) {
					ReloadAllowedWarehousesFrom();
				}
				return _allowedWarehousesFrom;
			}
		}

		public IEnumerable<Warehouse> AllowedWarehousesTo {
			get {
				if(_allowedWarehousesTo == null) {
					ReloadAllowedWarehousesTo();
				}
				return _allowedWarehousesTo;
			}
		}

		public bool CanSelectWarehouseTo => Entity.FromWarehouse != null;

		public IEnumerable<Warehouse> WarehousesTo {
			get {
				var result = new List<Warehouse>(AllowedWarehousesTo);
				if(Entity.ToWarehouse != null && !AllowedWarehousesTo.Contains(Entity.ToWarehouse)) {
					result.Add(Entity.ToWarehouse);
				}
				return result;
			}
		}

		public bool CanChangeDocumentTypeByStorageAndStorageFrom => CanEditNewDocument && !Entity.Items.Any();

		public IEnumerable<Warehouse> WarehousesFrom {
			get {
				var result = new List<Warehouse>(AllowedWarehousesFrom);
				if(Entity.FromWarehouse != null && !AllowedWarehousesFrom.Contains(Entity.FromWarehouse)) {
					result.Add(Entity.FromWarehouse);
				}
				return result;
			}
		}

		public bool CanShowWarehouseFrom => Entity.StorageFrom == StorageType.Warehouse;
		public bool CanShowEmployeeFrom => Entity.StorageFrom == StorageType.Employee;
		public bool CanShowCarFrom => Entity.StorageFrom == StorageType.Car;
		
		public bool CanShowWarehouseTo => Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToWarehouse;
		public bool CanShowEmployeeTo => Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToEmployee;
		public bool CanShowCarTo => Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToCar;

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(
				e => e.CanSend,
				() => CanSend);
			SetPropertyChangeRelation(
				e => e.CanReceive,
				() => CanReceive);
			SetPropertyChangeRelation(
				e => e.CanAcceptDiscrepancy,
				() => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(
				e => e.Status,
				() => CanEditNewDocument,
				() => CanChangeTargetWarehouseDocument);
			SetPropertyChangeRelation(
				e => e.ToWarehouse,
				() => CanSend,
				() => CanReceive,
				() => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(
				e => e.FromWarehouse,
				() => CanSend,
				() => CanReceive,
				() => CanAcceptDiscrepancy,
				() => CanAddItem,
				() => CanFillFromOrders);
			SetPropertyChangeRelation(
				e => e.ToEmployee,
				() => CanSend,
				() => CanReceive,
				() => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(
				e => e.FromEmployee,
				() => CanSend,
				() => CanReceive,
				() => CanAcceptDiscrepancy,
				() => CanAddItem);
			SetPropertyChangeRelation(
				e => e.ToCar,
				() => CanSend,
				() => CanReceive,
				() => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(
				e => e.FromCar,
				() => CanSend,
				() => CanReceive,
				() => CanAcceptDiscrepancy,
				() => CanAddItem);
			SetPropertyChangeRelation(e => e.CanAddItem,
				() => CanAddItem,
				() => CanFillFromOrders);
			SetPropertyChangeRelation(
				e => e.CanDeleteItems,
				() => CanDeleteItems);

			//TODO: неиспользуемое свойство CanSelectWarehouseTo
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSelectWarehouseTo);
			
			SetPropertyChangeRelation(
				e => e.StorageFrom,
				() => CanShowWarehouseFrom,
				() => CanShowEmployeeFrom,
				() => CanShowCarFrom);
			SetPropertyChangeRelation(
				e => e.MovementDocumentTypeByStorage,
				() => CanChangeWagon,
				() => CanShowWarehouseTo,
				() => CanShowEmployeeTo,
				() => CanShowCarTo);
			OnEntityPropertyChanged(ReloadAllowedWarehousesTo, e => e.FromWarehouse);
		}

		private void SetDefaultWarehouseFrom()
		{
			if(CurrentUserSettings?.DefaultWarehouse == null)
			{
				return;
			}

			Entity.FromWarehouse = CurrentUserSettings.DefaultWarehouse;
		}
		
		private void ResolveInnerDependencies()
		{
			var entityExtendedPermissionValidator = _scope.Resolve<IEntityExtendedPermissionValidator>();
			var warehouseRepository = _scope.Resolve<IWarehouseRepository>();

			SetPermissions(entityExtendedPermissionValidator);
			ValidationContext.ServiceContainer.AddService(typeof(IWarehouseRepository), warehouseRepository);
		}

		private void SetPermissions(IEntityExtendedPermissionValidator entityExtendedPermissionValidator)
		{
			_canEditRectroactively = entityExtendedPermissionValidator
				.Validate(typeof(MovementDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			_canChangeAcceptedMovementDoc =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_accepted_movement_doc");
			_canAcceptMovementDocumentDiscrepancy =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("can_accept_movement_document_dicrepancy");
			HasAccessToEmployeeStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_employee_storage_in_warehouse_documents");
			HasAccessToCarStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_car_storage_in_warehouse_documents");
			_canEditStoreMovementDocumentTransporterData =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.StorePermissions.Documents.CanEditStoreMovementDocumentTransporterData);
		}
		
		private void SetStoragesViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<MovementDocument>(this, Entity, UoW, NavigationManager, _scope);
			
			WagonEntryViewModel = builder.ForProperty(x => x.MovementWagon)
				.UseViewModelDialog<MovementWagonViewModel>()
				.UseViewModelJournalAndAutocompleter<MovementWagonJournalViewModel>()
				.Finish();
			
			FromEmployeeStorageEntryViewModel = builder.ForProperty(x => x.FromEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			
			ToEmployeeStorageEntryViewModel = builder.ForProperty(x => x.ToEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking
				)
				.Finish();
			
			FromCarStorageEntryViewModel = builder.ForProperty(x => x.FromCar)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
			
			ToCarStorageEntryViewModel = builder.ForProperty(x => x.ToCar)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
			.Finish();
		}

		private void ReloadAllowedWarehousesFrom()
		{
			var allowedWarehouses =
				_warehousePermissionValidator.GetAllowedWarehouses(WarehousePermissionsType.MovementEdit, CurrentEmployee);
			_allowedWarehousesFrom = UoW.Session.QueryOver<Warehouse>()
				.Where(x => !x.IsArchive)
				.WhereRestrictionOn(x => x.Id).IsIn(allowedWarehouses.Select(x => x.Id).ToArray())
				.List();
			OnPropertyChanged(nameof(AllowedWarehousesFrom));
			OnPropertyChanged(nameof(WarehousesFrom));
		}

		private void ReloadAllowedWarehousesTo()
		{
			var allowedWarehouses = UoW.GetAll<Warehouse>().Where(x => !x.IsArchive).ToList();
			if(allowedWarehouses.Contains(Entity.FromWarehouse)) {
				allowedWarehouses.Remove(Entity.FromWarehouse);
			}
			if(!allowedWarehouses.Contains(Entity.ToWarehouse)) {
				Entity.ToWarehouse = null;
			}
			_allowedWarehousesTo = allowedWarehouses;
			OnPropertyChanged(nameof(AllowedWarehousesTo));
			OnPropertyChanged(nameof(WarehousesTo));
		}

		#endregion Warehouses

		public Employee CurrentEmployee =>
			_currentEmployee ??
			(_currentEmployee = _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId));

		public UserSettings CurrentUserSettings =>
			_currentUserSettings ??
			(_currentUserSettings = _userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId));

		public override bool Save(bool close)
		{
			if(UoW.IsNew) {
				Entity.AuthorId = CurrentEmployee?.Id;
				Entity.SetTimeStamp(DateTime.Now);
			}

			Entity.LastEditorId = CurrentEmployee?.Id;
			Entity.LastEditedTime = DateTime.Now;

			return base.Save(close);
		}

		public bool CanEditSentAmount => CanSend;
		public bool CanEditReceivedAmount => CanReceive;
		public bool CanEditNewDocument => CanEdit && Entity.NewOrSentStatus;
		public bool CanChangeTargetWarehouseDocument => CanEditNewDocument || _canEditRectroactively;
		public bool HasAccessToEmployeeStorages { get; private set; }
		public bool HasAccessToCarStorages { get; private set; }
		public bool CanEditStoreMovementDocumentTransporterData => _canEditStoreMovementDocumentTransporterData;

		public bool CanChangeWagon =>
			Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToWarehouse &&
			(CanEditNewDocument ||
				(Entity.Status == MovementDocumentStatus.Accepted && PermissionResult.CanUpdate && _canChangeAcceptedMovementDoc));

		public bool CanVisibleWagon => Entity.DocumentType == MovementDocumentType.Transportation;

		#region Commands

		public bool CanSend => CanEdit && Entity.CanSend && HasAccessToStorageFrom;

		public DelegateCommand SendCommand {
			get {
				if(_sendCommand == null) {
					_sendCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.Send(CurrentEmployee);
							SaveAndClose();
						},
						() => CanSend
					);
					_sendCommand.CanExecuteChangedWith(this, x => x.CanSend);
				}
				return _sendCommand;
			}
		}

		public bool CanReceive => CanEdit && Entity.CanReceive && HasAccessToStorageTo;

		public DelegateCommand ReceiveCommand {
			get {
				if(_receiveCommand == null) {
					_receiveCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.Receive(CurrentEmployee, UoW);
							SaveAndClose();
						}, 
						() => CanReceive
					);
					_receiveCommand.CanExecuteChangedWith(this, x => x.CanReceive);
				}
				return _receiveCommand;
			}
		}

		public bool CanAcceptDiscrepancy => CanEdit
			&& Entity.CanAcceptDiscrepancy
			&& _canAcceptMovementDocumentDiscrepancy
			&& _warehousePermissionValidator.Validate(WarehousePermissionsType.MovementEdit, Entity.FromWarehouse, CurrentEmployee);

		public DelegateCommand AcceptDiscrepancyCommand {
			get {
				if(_acceptDiscrepancyCommand == null) {
					_acceptDiscrepancyCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.AcceptDiscrepancy(CurrentEmployee);
							SaveAndClose();
						},
						() => CanAcceptDiscrepancy
					);
					_acceptDiscrepancyCommand.CanExecuteChangedWith(this, x => x.CanAcceptDiscrepancy);
				}
				return _acceptDiscrepancyCommand;
			}
		}

		public bool CanAddItem =>
			CanEdit && Entity.CanAddItem && (Entity.FromWarehouse != null || Entity.FromEmployee != null || Entity.FromCar != null);

		public DelegateCommand AddItemCommand
		{
			get
			{
				if(_addItemCommand == null)
				{
					_addItemCommand = new DelegateCommand(
						() =>
						{
							var alreadyAddedNomenclatures =
								Entity.Items.Where(x => x.Nomenclature != null && !x.Nomenclature.HasInventoryAccounting)
									.Select(x => x.Nomenclature.Id);

							var filterParams = GetNomenclatureStockFilterByStorage(alreadyAddedNomenclatures);
							
							var nomenclatureStockBalanceJournal = NavigationManager
								.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(
									this, filterParams, OpenPageOptions.AsSlave).ViewModel;
							nomenclatureStockBalanceJournal.SelectionMode = JournalSelectionMode.Multiple;
							nomenclatureStockBalanceJournal.OnEntitySelectedResult += (sender, e) =>
							{
								var selectedNodes = e.SelectedNodes.Cast<NomenclatureStockJournalNode>();
								if(!selectedNodes.Any())
								{
									return;
								}
								
								var selectedNomenclatures =
									UoW.GetById<Nomenclature>(selectedNodes.Select(x => x.Id))
										.Where(x => alreadyAddedNomenclatures.All(y => y != x.Id));

								foreach(var nomenclature in selectedNomenclatures)
								{
									Entity.AddItem(nomenclature, 0, selectedNodes.FirstOrDefault(x => x.Id == nomenclature.Id).StockAmount);
								}

								FireItemsChanged();
							};
						},
						() => CanAddItem
					);
					_addItemCommand.CanExecuteChangedWith(this, x => x.CanAddItem);
				}
				return _addItemCommand;
			}
		}

		public bool CanDeleteItems => CanEdit && Entity.CanDeleteItems;

		public DelegateCommand<MovementDocumentItem> DeleteItemCommand {
			get {
				if(_deleteItemCommand == null) {
					_deleteItemCommand = new DelegateCommand<MovementDocumentItem>(
						selectedItem => {
							Entity.DeleteItem(selectedItem);
							FireItemsChanged();
						},
						selectedItem => CanDeleteItems && selectedItem != null
					);
					_deleteItemCommand.CanExecuteChangedWith(this, x => x.CanDeleteItems);
				}
				return _deleteItemCommand;
			}
		}

		public bool CanFillFromOrders => CanEdit && Entity.CanAddItem && Entity.FromWarehouse != null;

		public DelegateCommand FillFromOrdersCommand
		{
			get
			{
				if(_fillFromOrdersCommand == null)
				{
					_fillFromOrdersCommand = new DelegateCommand(
						() =>
						{
							var isOnlineStoreOrders = true;
							var orderStatuses = new[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading };
							var orderSelector = _orderSelectorFactory.CreateOrderSelectorForDocument(isOnlineStoreOrders, orderStatuses);
							
							orderSelector.OnEntitySelectedResult += (sender, e) =>
							{
								var selectedNodes = e.SelectedNodes.Cast<OrderForMovDocJournalNode>();
								if(!selectedNodes.Any())
								{
									return;
								}
								var orders = UoW.GetById<Order>(selectedNodes.Select(x => x.Id));
								var orderItems = orders.SelectMany(x => x.OrderItems);
								var nomIds = orderItems.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature.Id).ToList();

								var nomsAmount = new Dictionary<int, decimal>();
								if (nomIds != null && nomIds.Any())
								{
									nomIds = nomIds.Distinct().ToList();
									nomsAmount =
										_stockRepository.NomenclatureInStock(UoW, nomIds.ToArray(), new []{ Entity.FromWarehouse.Id });
								}
								foreach(var item in orderItems)
								{
									var moveItem = Entity.Items.FirstOrDefault(x => x.Nomenclature.Id == item.Nomenclature.Id);
									if (moveItem == null)
									{
										var count = item.Count > nomsAmount[item.Nomenclature.Id] ? nomsAmount[item.Nomenclature.Id] : item.Count;
										if(count == 0)
										{
											continue;
										}

										Entity.AddItem(item.Nomenclature, count, nomsAmount[item.Nomenclature.Id]);
									}
									else
									{
										var count = (moveItem.SentAmount + item.Count) > nomsAmount[item.Nomenclature.Id] ?
											nomsAmount[item.Nomenclature.Id] :
											(moveItem.SentAmount + item.Count);
										if(count == 0)
										{
											continue;
										}

										moveItem.SentAmount = count;
									}
								}

								FireItemsChanged();
							};
							TabParent.AddSlaveTab(this, orderSelector);
						},
						() => CanFillFromOrders
					);
					_fillFromOrdersCommand.CanExecuteChangedWith(this, x => x.CanFillFromOrders);
				}
				return _fillFromOrdersCommand;
			}
		}

		public DelegateCommand PrintCommand {
			get {
				if(_printCommand == null) {
					_printCommand = new DelegateCommand(
						() => {
							if(Entity.Status == MovementDocumentStatus.New && SendCommand.CanExecute()) {
								if(CommonServices.InteractiveService.Question("Перед печатью необходимо отправить перемещение. Отправить?", "Печать документа перемещения")) {
									SendCommand.Execute();
									var doc = new MovementDocumentRdl(Entity);
									_rdlPreviewOpener.OpenRldDocument(typeof(MovementDocument), doc);
								}
							}
							else if(Entity.Status != MovementDocumentStatus.New && !UoW.IsNew) {
								var doc = new MovementDocumentRdl(Entity);
								_rdlPreviewOpener.OpenRldDocument(typeof(MovementDocument), doc);
							}
						},
						() => (Entity.Status == MovementDocumentStatus.New && SendCommand.CanExecute()) || Entity.Status != MovementDocumentStatus.New
					);
					_printCommand.CanExecuteChangedWith(this, x => x.CanSend);
				}
				return _printCommand;
			}
		}
		
		public DelegateCommand AddInventoryInstanceCommand =>
			_addInventoryInstanceCommand ?? (_addInventoryInstanceCommand = new DelegateCommand(
				() =>
				{
					var filterParams = GetInstancesStockBalanceFilterByStorage();
					
					var page =
						NavigationManager
							.OpenViewModel<InventoryInstancesStockBalanceJournalViewModel, Action<InventoryInstancesStockBalanceJournalFilterViewModel>>(
								this, filterParams, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
					page.ViewModel.OnSelectResult += OnInventoryInstanceSelectResult;
				}));

		#endregion Commands

		private bool HasAccessToStorageTo
		{
			get
			{
				switch(Entity.MovementDocumentTypeByStorage)
				{
					case MovementDocumentTypeByStorage.ToWarehouse:
						return _warehousePermissionValidator.Validate(
							WarehousePermissionsType.MovementEdit, Entity.ToWarehouse, CurrentEmployee);
					case MovementDocumentTypeByStorage.ToEmployee:
						return HasAccessToEmployeeStorages;
					case MovementDocumentTypeByStorage.ToCar:
						return HasAccessToCarStorages;
				}
				
				return false;
			}
		}
		
		private bool HasAccessToStorageFrom
		{
			get
			{
				switch(Entity.StorageFrom)
				{
					case StorageType.Warehouse:
						return _warehousePermissionValidator.Validate(
							WarehousePermissionsType.MovementEdit, Entity.FromWarehouse, CurrentEmployee);
					case StorageType.Employee:
						return HasAccessToEmployeeStorages;
					case StorageType.Car:
						return HasAccessToCarStorages;
				}
				
				return false;
			}
		}

		public IEntityEntryViewModel SourceWarehouseViewModel { get; }
		public IEntityEntryViewModel TargetWarehouseViewModel { get; }

		private void OnInventoryInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedItems = e.GetSelectedObjects<InventoryInstancesStockJournalNode>();

			if(!selectedItems.Any())
			{
				return;
			}

			foreach(var item in selectedItems)
			{
				if(Entity.ObservableItems.OfType<InstanceMovementDocumentItem>()
					.Any(x => x.InventoryNomenclatureInstance.Id == item.Id))
				{
					continue;
				}

				var inventoryInstance = _nomenclatureInstanceRepository.GetInventoryNomenclatureInstance(UoW, item.Id);

				Entity.AddItem(inventoryInstance, 1, item.Balance);
			}
			
			FireItemsChanged();
		}
		
		private Action<NomenclatureStockFilterViewModel> GetNomenclatureStockFilterByStorage(IEnumerable<int> alreadyAddedNomenclatures)
		{
			Action<NomenclatureStockFilterViewModel> filterParams = null;
			
			switch(Entity.StorageFrom)
			{
				case StorageType.Warehouse:
					return filterParams = f =>
					{
						f.RestrictWarehouse = Entity.FromWarehouse;
						f.ExcludedNomenclatureIds = alreadyAddedNomenclatures;
					};
				case StorageType.Employee:
					return filterParams = f =>
					{
						f.RestrictEmployeeStorage = Entity.FromEmployee;
						f.ExcludedNomenclatureIds = alreadyAddedNomenclatures;
					};
				case StorageType.Car:
					return filterParams = f =>
					{
						f.RestrictCarStorage = Entity.FromCar;
						f.ExcludedNomenclatureIds = alreadyAddedNomenclatures;
					};
				default:
					return filterParams;
			}
		}
		
		private Action<InventoryInstancesStockBalanceJournalFilterViewModel> GetInstancesStockBalanceFilterByStorage()
		{
			Action<InventoryInstancesStockBalanceJournalFilterViewModel> filterParams = null;
			
			switch(Entity.StorageFrom)
			{
				case StorageType.Warehouse:
					return filterParams = f =>
					{
						f.IsShow = true;
						f.RestrictedStorageType = StorageType.Warehouse;
						f.RestrictedWarehouse = Entity.FromWarehouse;
					};
				case StorageType.Employee:
					return filterParams = f =>
					{
						f.IsShow = true;
						f.RestrictedStorageType = StorageType.Employee;
						f.RestrictedEmployeeStorage = Entity.FromEmployee;
					};
				case StorageType.Car:
					return filterParams = f =>
					{
						f.IsShow = true;
						f.RestrictedStorageType = StorageType.Car;
						f.RestrictedCarStorage = Entity.FromCar;
					};
				default:
					return filterParams;
			}
		}

		private void FireItemsChanged()
		{
			OnPropertyChanged(nameof(CanSend));
			OnPropertyChanged(nameof(CanReceive));
			OnPropertyChanged(nameof(CanAcceptDiscrepancy));
			OnPropertyChanged(nameof(CanChangeDocumentTypeByStorageAndStorageFrom));
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= OnMovementDocumentPropertyChanged;

			base.Dispose();
		}
	}
}
