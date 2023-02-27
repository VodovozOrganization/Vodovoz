using System;
using System.Collections.Generic;
using System.Linq;
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
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Journals;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.PermissionExtensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Store;

namespace Vodovoz.ViewModels.Warehouses
{
	public class MovementDocumentViewModel : EntityTabViewModelBase<MovementDocument>
	{
		private readonly ILifetimeScope _scope;
		private IEmployeeService _employeeService;
		private INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private IOrderSelectorFactory _orderSelectorFactory;
		private IUserRepository _userRepository;
		private IRDLPreviewOpener _rdlPreviewOpener;
		private IStockRepository _stockRepository;
		private IWarehousePermissionValidator _warehousePermissionValidator;
		private INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private bool _canEditRectroactively;
		private bool _canChangeAcceptedMovementDoc;
		private bool _canAcceptMovementDocumentDiscrepancy;

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
			ILifetimeScope scope) 
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			ResolveInnerDependencies();
			SetStoragesViewModels();
			ConfigureEntityChangingRelations();

			if(UoW.IsNew)
			{
				Entity.DocumentType = MovementDocumentType.Transportation;
				SetDefaultWarehouseFrom();
			}
		}

		private void ResolveInnerDependencies()
		{
			_employeeService = _scope.Resolve<IEmployeeService>();
			_nomenclatureSelectorFactory = _scope.Resolve<INomenclatureJournalFactory>();
			_orderSelectorFactory = _scope.Resolve<IOrderSelectorFactory>();
			_userRepository = _scope.Resolve<IUserRepository>();
			_rdlPreviewOpener = _scope.Resolve<IRDLPreviewOpener>();
			_warehousePermissionValidator = _scope.Resolve<IWarehousePermissionValidator>();
			_stockRepository = _scope.Resolve<IStockRepository>();
			_nomenclatureInstanceRepository = _scope.Resolve<INomenclatureInstanceRepository>();
			
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
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
			
			ToEmployeeStorageEntryViewModel = builder.ForProperty(x => x.ToEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
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
		
		public IEntityEntryViewModel WagonEntryViewModel { get; private set; }
		public IEntityEntryViewModel FromEmployeeStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel ToEmployeeStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel FromCarStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel ToCarStorageEntryViewModel { get; private set; }

		public bool CanEdit => 
			(UoW.IsNew && PermissionResult.CanCreate)
			|| (PermissionResult.CanUpdate &&
			    (Entity.TimeStamp.Date >= DateTime.Today || DateTime.Today <= Entity.TimeStamp.Date.AddDays(4).AddHours(23).AddMinutes(59)))
			|| _canEditRectroactively;

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.CanSend, () => CanSend);
			SetPropertyChangeRelation(e => e.CanReceive, () => CanReceive);
			SetPropertyChangeRelation(e => e.CanAcceptDiscrepancy, () => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(e => e.Status, () => CanEditNewDocument);
			SetPropertyChangeRelation(e => e.ToWarehouse, () => CanSend, () => CanReceive, () => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSend, () => CanReceive, () => CanAcceptDiscrepancy);

			SetPropertyChangeRelation(e => e.CanAddItem, () => CanAddItem, () => CanFillFromOrders);
			SetPropertyChangeRelation(e => e.CanDeleteItems, () => CanDeleteItems);

			//TODO неиспользуемое свойство CanSelectWarehouseTo
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSelectWarehouseTo);
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanAddItem, () => CanFillFromOrders);
			SetPropertyChangeRelation(
				e => e.FromEmployee,
				() => CanAddItem);
			SetPropertyChangeRelation(
				e => e.FromCar,
				() => CanAddItem);
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

		#region Header info

		public string AuthorInfo {
			get {
				if(Entity.Author == null) {
					return null;
				}
				return $"{Entity.Author.GetPersonNameWithInitials()}, {Entity.TimeStamp:dd.MM.yyyy HH:mm}";
			}
		}

		public string LastEditorInfo {
			get {
				if(Entity.LastEditor == null) {
					return null;
				}
				return $"{Entity.LastEditor.GetPersonNameWithInitials()}, {Entity.LastEditedTime:dd.MM.yyyy HH:mm}";
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

		private IEnumerable<Warehouse> _allowedWarehousesFrom;
		public IEnumerable<Warehouse> AllowedWarehousesFrom {
			get {
				if(_allowedWarehousesFrom == null) {
					ReloadAllowedWarehousesFrom();
				}
				return _allowedWarehousesFrom;
			}
		}

		private IEnumerable<Warehouse> _allowedWarehousesTo;
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

		public bool CanShowWarehouseFrom => Entity.StorageFrom == Storage.Warehouse;
		public bool CanShowEmployeeFrom => Entity.StorageFrom == Storage.Employee;
		public bool CanShowCarFrom => Entity.StorageFrom == Storage.Car;
		
		public bool CanShowWarehouseTo => Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ForWarehouse;
		public bool CanShowEmployeeTo => Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ForEmployee;
		public bool CanShowCarTo => Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ForCar;

		private void SetDefaultWarehouseFrom()
		{
			if(CurrentUserSettings == null || CurrentUserSettings.DefaultWarehouse == null) {
				return;
			}

			Entity.FromWarehouse = CurrentUserSettings.DefaultWarehouse;
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

		private Employee _currentEmployee;
		public Employee CurrentEmployee =>
			_currentEmployee ??
			(_currentEmployee = _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId));

		private UserSettings _currentUserSettings;

		public UserSettings CurrentUserSettings =>
			_currentUserSettings ??
			(_currentUserSettings = _userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId));

		public override bool Save(bool close)
		{
			if(UoW.IsNew) {
				Entity.Author = CurrentEmployee;
				Entity.TimeStamp = DateTime.Now;
			}

			Entity.LastEditor = CurrentEmployee;
			Entity.LastEditedTime = DateTime.Now;

			return base.Save(close);
		}

		public bool CanEditSentAmount => CanSend;
		public bool CanEditReceivedAmount => CanReceive;

		public bool CanEditNewDocument =>
			CanEdit && (Entity.Status == MovementDocumentStatus.New || Entity.Status == MovementDocumentStatus.Sended);

		public bool CanChangeWagon =>
			Entity.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ForWarehouse &&
			(CanEditNewDocument ||
				(Entity.Status == MovementDocumentStatus.Accepted && PermissionResult.CanUpdate && _canChangeAcceptedMovementDoc));

		public bool CanVisibleWagon => Entity.DocumentType == MovementDocumentType.Transportation;

		#region Commands

		public bool CanSend => CanEdit
			&& Entity.CanSend
			&& _warehousePermissionValidator.Validate(WarehousePermissionsType.MovementEdit, Entity.FromWarehouse, CurrentEmployee);

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
					_sendCommand.CanExecuteChanged += (sender, e) => PrintCommand.RaiseCanExecuteChanged();
				}
				return _sendCommand;
			}
		}

		public bool CanReceive => CanEdit
			&& Entity.CanReceive
			&& _warehousePermissionValidator.Validate(WarehousePermissionsType.MovementEdit, Entity.ToWarehouse, CurrentEmployee);

		public DelegateCommand ReceiveCommand {
			get {
				if(_receiveCommand == null) {
					_receiveCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.Receive(CurrentEmployee);
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

		public DelegateCommand AddItemCommand {
			get {
				if(_addItemCommand == null) {
					_addItemCommand = new DelegateCommand(
						() => {
							var alreadyAddedNomenclatures = Entity.Items.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature.Id);
							var nomenclatureSelector = _nomenclatureSelectorFactory.CreateNomenclatureSelectorForWarehouse(Entity.FromWarehouse, alreadyAddedNomenclatures);
							nomenclatureSelector.OnEntitySelectedResult += (sender, e) => {
								IEnumerable<NomenclatureStockJournalNode> selectedNodes = e.SelectedNodes.Cast<NomenclatureStockJournalNode>();
								if(!selectedNodes.Any()) {
									return;
								}
								var existedItems = Entity.Items.Select(x => x.Nomenclature.Id);
								var selectedNomenclatures = UoW.GetById<Nomenclature>(selectedNodes.Select(x => x.Id)).Where(x => existedItems.All(y => y != x.Id));

								foreach(var nomenclature in selectedNomenclatures) {
									Entity.AddItem(nomenclature, 0, selectedNodes.FirstOrDefault(x => x.Id == nomenclature.Id).StockAmount);
								}
								OnPropertyChanged(nameof(CanSend));
								OnPropertyChanged(nameof(CanReceive));
								OnPropertyChanged(nameof(CanAcceptDiscrepancy));
								OnPropertyChanged(nameof(CanChangeDocumentTypeByStorageAndStorageFrom));
							};
							TabParent.AddSlaveTab(this, nomenclatureSelector);
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
							OnPropertyChanged(nameof(CanSend));
							OnPropertyChanged(nameof(CanReceive));
							OnPropertyChanged(nameof(CanAcceptDiscrepancy));
							OnPropertyChanged(nameof(CanChangeDocumentTypeByStorageAndStorageFrom));
						},
						selectedItem => CanDeleteItems && selectedItem != null
					);
					_deleteItemCommand.CanExecuteChangedWith(this, x => x.CanDeleteItems);
				}
				return _deleteItemCommand;
			}
		}

		public bool CanFillFromOrders => CanEdit && Entity.CanAddItem && Entity.FromWarehouse != null;

		public DelegateCommand FillFromOrdersCommand {
			get {
				if(_fillFromOrdersCommand == null) {
					_fillFromOrdersCommand = new DelegateCommand(
						() => {
							bool IsOnlineStoreOrders = true;
							IEnumerable<OrderStatus> orderStatuses = new OrderStatus[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading };
							var orderSelector = _orderSelectorFactory.CreateOrderSelectorForDocument(IsOnlineStoreOrders, orderStatuses);
							orderSelector.OnEntitySelectedResult += (sender, e) => {
								IEnumerable<OrderForMovDocJournalNode> selectedNodes = e.SelectedNodes.Cast<OrderForMovDocJournalNode>();
								if(!selectedNodes.Any()) {
									return;
								}
								var orders = UoW.GetById<Order>(selectedNodes.Select(x => x.Id));
								var orderItems = orders.SelectMany(x => x.OrderItems);
								var nomIds = orderItems.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature.Id).ToList();

								var nomsAmount = new Dictionary<int, decimal>();
								if (nomIds != null && nomIds.Any()) {
									nomIds = nomIds.Distinct().ToList();
									nomsAmount = _stockRepository.NomenclatureInStock(UoW, nomIds.ToArray(), Entity.FromWarehouse.Id);
								}
								foreach(var item in orderItems) {
									var moveItem = Entity.Items.FirstOrDefault(x => x.Nomenclature.Id == item.Nomenclature.Id);
									if (moveItem == null) {
										var count = item.Count > nomsAmount[item.Nomenclature.Id] ? nomsAmount[item.Nomenclature.Id] : item.Count;
										if(count == 0)
										{
											continue;
										}

										Entity.AddItem(item.Nomenclature, count, nomsAmount[item.Nomenclature.Id]);
									} else {
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
								OnPropertyChanged(nameof(CanSend));
								OnPropertyChanged(nameof(CanReceive));
								OnPropertyChanged(nameof(CanAcceptDiscrepancy));
								OnPropertyChanged(nameof(CanChangeDocumentTypeByStorageAndStorageFrom));
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
					var page =
						NavigationManager.OpenViewModel<InventoryInstancesStockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += OnInventoryInstanceSelectResult;
				}));

		#endregion Commands
		
		private void OnInventoryInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedItem = e.GetSelectedObjects<InventoryInstancesStockJournalNode>().FirstOrDefault();

			if(selectedItem is null)
			{
				return;
			}

			if(Entity.ObservableItems.Any(x => x.ItemEntityId == selectedItem.Id))
			{
				return;
			}

			var inventoryInstance = _nomenclatureInstanceRepository.GetInventoryNomenclatureInstance(UoW, selectedItem.Id);

			Entity.AddItem(inventoryInstance, 1, 1);
			OnPropertyChanged(nameof(CanChangeDocumentTypeByStorageAndStorageFrom));
		}
	}
}
