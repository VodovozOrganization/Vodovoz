using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.ViewModels;
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
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.PermissionExtensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Warehouses
{
	public class MovementDocumentViewModel : EntityTabViewModelBase<MovementDocument>
	{
		private readonly IEmployeeService employeeService;
		private readonly INomenclatureJournalFactory nomenclatureSelectorFactory;
		private readonly IOrderSelectorFactory orderSelectorFactory;
		private readonly IUserRepository userRepository;
		private readonly IRDLPreviewOpener rdlPreviewOpener;
		private readonly IStockRepository _stockRepository;
		private readonly IWarehousePermissionValidator warehousePermissionValidator;
		private readonly bool canEditRectroactively;

		public MovementDocumentViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehousePermissionService warehousePermissionService,
			IEmployeeService employeeService,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			IOrderSelectorFactory orderSelectorFactory,
			IWarehouseRepository warehouseRepository,
			IUserRepository userRepository,
			IRDLPreviewOpener rdlPreviewOpener,
			ICommonServices commonServices,
			IStockRepository stockRepository) 
		: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			warehousePermissionValidator = warehousePermissionService.GetValidator();
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));

			if(warehouseRepository is null)
			{
				throw new ArgumentNullException(nameof(warehouseRepository));
			}
			
			canEditRectroactively =
				(entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator)))
				.Validate(typeof(MovementDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			
			ConfigureEntityChangingRelations();
			if(UoW.IsNew)
			{
				Entity.DocumentType = MovementDocumentType.Transportation;
				SetDefaultWarehouseFrom();
			}
			ValidationContext.ServiceContainer.AddService(typeof(IWarehouseRepository), warehouseRepository);
		}

		public bool CanEdit => 
			(UoW.IsNew && PermissionResult.CanCreate) 
			|| (PermissionResult.CanUpdate && (Entity.TimeStamp.Date >= DateTime.Today || DateTime.Today <= Entity.TimeStamp.Date.AddDays(4).AddHours(23).AddMinutes(59) ))
			|| canEditRectroactively;

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

			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSelectWarehouseTo);
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanAddItem, () => CanFillFromOrders);
			OnEntityPropertyChanged(ReloadAllowedWarehousesTo, e => e.FromWarehouse);

			Entity.ObservableItems.ElementAdded += (aList, aIdx) => OnPropertyChanged(nameof(CanChangeWarehouseFrom));
			Entity.ObservableItems.ElementRemoved += (aList, aIdx, aObject) => OnPropertyChanged(nameof(CanChangeWarehouseFrom));
		}

		#region Header info

		public string AuthorInfo {
			get {
				if(Entity.Author == null) {
					return null;
				}
				return $"{Entity.Author.GetPersonNameWithInitials()}, {Entity.TimeStamp.ToString("dd.MM.yyyy HH:mm")}";
			}
		}

		public string LastEditorInfo {
			get {
				if(Entity.LastEditor == null) {
					return null;
				}
				return $"{Entity.LastEditor.GetPersonNameWithInitials()}, {Entity.LastEditedTime.ToString("dd.MM.yyyy HH:mm")}";
			}
		}

		public string SendedInfo {
			get {
				if(Entity.Sender == null || Entity.SendTime == null) {
					return null;
				}
				return $"{Entity.Sender.GetPersonNameWithInitials()}, {Entity.SendTime.Value.ToString("dd.MM.yyyy HH:mm")}";
			}
		}

		public string ReceiverInfo {
			get {
				if(Entity.Receiver == null || Entity.ReceiveTime == null) {
					return null;
				}
				return $"{Entity.Receiver.GetPersonNameWithInitials()}, {Entity.ReceiveTime.Value.ToString("dd.MM.yyyy HH:mm")}";
			}
		}

		public string DiscrepancyAccepterInfo {
			get {
				if(Entity.DiscrepancyAccepter == null || Entity.DiscrepancyAcceptTime == null) {
					return null;
				}
				return $"{Entity.DiscrepancyAccepter.GetPersonNameWithInitials()}, {Entity.DiscrepancyAcceptTime.Value.ToString("dd.MM.yyyy HH:mm")}";
			}
		}

		#endregion Header info

		#region Warehouses

		private IEnumerable<Warehouse> allowedWarehousesFrom;
		public IEnumerable<Warehouse> AllowedWarehousesFrom {
			get {
				if(allowedWarehousesFrom == null) {
					ReloadAllowedWarehousesFrom();
				}
				return allowedWarehousesFrom;
			}
		}

		private IEnumerable<Warehouse> allowedWarehousesTo;
		public IEnumerable<Warehouse> AllowedWarehousesTo {
			get {
				if(allowedWarehousesTo == null) {
					ReloadAllowedWarehousesTo();
				}
				return allowedWarehousesTo;
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

		public bool CanChangeWarehouseFrom => CanEditNewDocument && !Entity.Items.Any();

		public IEnumerable<Warehouse> WarehousesFrom {
			get {
				var result = new List<Warehouse>(AllowedWarehousesFrom);
				if(Entity.FromWarehouse != null && !AllowedWarehousesFrom.Contains(Entity.FromWarehouse)) {
					result.Add(Entity.FromWarehouse);
				}
				return result;
			}
		}

		private void SetDefaultWarehouseFrom()
		{
			if(CurrentUserSettings == null || CurrentUserSettings.DefaultWarehouse == null) {
				return;
			}

			Entity.FromWarehouse = CurrentUserSettings.DefaultWarehouse;
		}

		private void ReloadAllowedWarehousesFrom()
		{
			var allowedWarehouses = warehousePermissionValidator.GetAllowedWarehouses(WarehousePermissionsType.MovementEdit, CurrentEmployee);
			allowedWarehousesFrom = UoW.Session.QueryOver<Warehouse>()
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
			allowedWarehousesTo = allowedWarehouses;
			OnPropertyChanged(nameof(AllowedWarehousesTo));
			OnPropertyChanged(nameof(WarehousesTo));
		}

		#endregion Warehouses


		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		private UserSettings currentUserSettings;
		public UserSettings CurrentUserSettings {
			get {
				if(currentUserSettings == null) {
					currentUserSettings = userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId);
				}
				return currentUserSettings;
			}
		}

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

		public bool CanEditSendedAmount => CanSend;

		public bool CanEditReceivedAmount => CanReceive;

		public bool CanEditNewDocument => CanEdit && (Entity.Status == MovementDocumentStatus.New || Entity.Status == MovementDocumentStatus.Sended);

		public bool CanChangeWagon => CanEditNewDocument || (Entity.Status == MovementDocumentStatus.Accepted && PermissionResult.CanUpdate &&
			CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_accepted_movement_doc"));

		public bool CanVisibleWagon => Entity.DocumentType == MovementDocumentType.Transportation;

		#region Commands

		public bool CanSend => CanEdit
			&& Entity.CanSend
			&& warehousePermissionValidator.Validate(WarehousePermissionsType.MovementEdit, Entity.FromWarehouse, CurrentEmployee);

		private DelegateCommand sendCommand;
		public DelegateCommand SendCommand {
			get {
				if(sendCommand == null) {
					sendCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.Send(CurrentEmployee);
							SaveAndClose();
						},
						() => CanSend
					);
					sendCommand.CanExecuteChangedWith(this, x => x.CanSend);
					sendCommand.CanExecuteChanged += (sender, e) => PrintCommand.RaiseCanExecuteChanged();
				}
				return sendCommand;
			}
		}

		public bool CanReceive => CanEdit
			&& Entity.CanReceive
			&& warehousePermissionValidator.Validate(WarehousePermissionsType.MovementEdit, Entity.ToWarehouse, CurrentEmployee);

		private DelegateCommand receiveCommand;
		public DelegateCommand ReceiveCommand {
			get {
				if(receiveCommand == null) {
					receiveCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.Receive(CurrentEmployee);
							SaveAndClose();
						}, 
						() => CanReceive
					);
					receiveCommand.CanExecuteChangedWith(this, x => x.CanReceive);
				}
				return receiveCommand;
			}
		}

		public bool CanAcceptDiscrepancy => CanEdit
			&& Entity.CanAcceptDiscrepancy
			&& CommonServices.PermissionService.ValidateUserPresetPermission("can_accept_movement_document_dicrepancy", CommonServices.UserService.CurrentUserId)
			&& warehousePermissionValidator.Validate(WarehousePermissionsType.MovementEdit, Entity.FromWarehouse, CurrentEmployee);

		private DelegateCommand acceptDiscrepancyCommand;
		public DelegateCommand AcceptDiscrepancyCommand {
			get {
				if(acceptDiscrepancyCommand == null) {
					acceptDiscrepancyCommand = new DelegateCommand(
						() => {
							if(!Validate()) {
								return;
							}
							Entity.AcceptDiscrepancy(CurrentEmployee);
							SaveAndClose();
						},
						() => CanAcceptDiscrepancy
					);
					acceptDiscrepancyCommand.CanExecuteChangedWith(this, x => x.CanAcceptDiscrepancy);
				}
				return acceptDiscrepancyCommand;
			}
		}

		public bool CanAddItem => CanEdit && Entity.CanAddItem && Entity.FromWarehouse != null;

		private DelegateCommand addItemCommand;
		public DelegateCommand AddItemCommand {
			get {
				if(addItemCommand == null) {
					addItemCommand = new DelegateCommand(
						() => {
							var alreadyAddedNomenclatures = Entity.Items.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature.Id);
							var nomenclatureSelector = nomenclatureSelectorFactory.CreateNomenclatureSelectorForWarehouse(Entity.FromWarehouse, alreadyAddedNomenclatures);
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
							};
							TabParent.AddSlaveTab(this, nomenclatureSelector);
						},
						() => CanAddItem
					);
					addItemCommand.CanExecuteChangedWith(this, x => x.CanAddItem);
				}
				return addItemCommand;
			}
		}

		public bool CanDeleteItems => CanEdit && Entity.CanDeleteItems;

		private DelegateCommand<MovementDocumentItem> deleteItemCommand;
		public DelegateCommand<MovementDocumentItem> DeleteItemCommand {
			get {
				if(deleteItemCommand == null) {
					deleteItemCommand = new DelegateCommand<MovementDocumentItem>(
						(selectedItem) => {
							Entity.DeleteItem(selectedItem);
							OnPropertyChanged(nameof(CanSend));
							OnPropertyChanged(nameof(CanReceive));
							OnPropertyChanged(nameof(CanAcceptDiscrepancy));
						},
						(selectedItem) => CanDeleteItems && selectedItem != null
					);
					deleteItemCommand.CanExecuteChangedWith(this, x => x.CanDeleteItems);
				}
				return deleteItemCommand;
			}
		}

		public bool CanFillFromOrders => CanEdit && Entity.CanAddItem && Entity.FromWarehouse != null;

		private DelegateCommand fillFromOrdersCommand;
		public DelegateCommand FillFromOrdersCommand {
			get {
				if(fillFromOrdersCommand == null) {
					fillFromOrdersCommand = new DelegateCommand(
						() => {
							bool IsOnlineStoreOrders = true;
							IEnumerable<OrderStatus> orderStatuses = new OrderStatus[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading };
							var orderSelector = orderSelectorFactory.CreateOrderSelectorForDocument(IsOnlineStoreOrders, orderStatuses);
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
								foreach (var item in orderItems) {
									var moveItem = Entity.Items.FirstOrDefault(x => x.Nomenclature.Id == item.Nomenclature.Id);
									if (moveItem == null) {
										var count = item.Count > nomsAmount[item.Nomenclature.Id] ? nomsAmount[item.Nomenclature.Id] : item.Count;
										if (count == 0)
											continue;
										Entity.AddItem(item.Nomenclature, count, nomsAmount[item.Nomenclature.Id]);
									} else {
										var count = (moveItem.SentAmount + item.Count) > nomsAmount[item.Nomenclature.Id] ?
											nomsAmount[item.Nomenclature.Id] :
											(moveItem.SentAmount + item.Count);
										if(count == 0)
											continue;
										moveItem.SentAmount = count;
									}
								}
								OnPropertyChanged(nameof(CanSend));
								OnPropertyChanged(nameof(CanReceive));
								OnPropertyChanged(nameof(CanAcceptDiscrepancy));
							};
							TabParent.AddSlaveTab(this, orderSelector);
						},
						() => CanFillFromOrders
					);
					fillFromOrdersCommand.CanExecuteChangedWith(this, x => x.CanFillFromOrders);
				}
				return fillFromOrdersCommand;
			}
		}

		private DelegateCommand printCommand;
		public DelegateCommand PrintCommand {
			get {
				if(printCommand == null) {
					printCommand = new DelegateCommand(
						() => {
							if(Entity.Status == MovementDocumentStatus.New && SendCommand.CanExecute()) {
								if(CommonServices.InteractiveService.Question("Перед печатью необходимо отправить перемещение. Отправить?", "Печать документа перемещения")) {
									SendCommand.Execute();
									var doc = new MovementDocumentRdl(Entity);
									if(doc is IPrintableRDLDocument) {
										rdlPreviewOpener.OpenRldDocument(typeof(MovementDocument), doc);
									}
								}
							} 
							else if(Entity.Status != MovementDocumentStatus.New && !UoW.IsNew) {
								var doc = new MovementDocumentRdl(Entity);
								if(doc is IPrintableRDLDocument) {
									rdlPreviewOpener.OpenRldDocument(typeof(MovementDocument), doc);
								}
							}
						},
						() => (Entity.Status == MovementDocumentStatus.New && SendCommand.CanExecute()) || Entity.Status != MovementDocumentStatus.New
					);
					printCommand.CanExecuteChangedWith(this, x => x.CanSend);
				}
				return printCommand;
			}
		}

		#endregion Commands
	}
}
