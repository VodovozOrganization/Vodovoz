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
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.PermissionExtensions;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Warehouses
{
	public class MovementDocumentViewModel : EntityTabViewModelBase<MovementDocument>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IEmployeeService employeeService;
		private readonly IEntityExtendedPermissionValidator entityExtendedPermissionValidator;
		private readonly INomenclatureSelectorFactory nomenclatureSelectorFactory;
		private readonly IWarehouseRepository warehouseRepository;
		private readonly IUserRepository userRepository;
		private readonly IRDLPreviewOpener rdlPreviewOpener;
		private IWarehousePermissionValidator warehousePermissionValidator;

		public MovementDocumentViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehousePermissionService warehousePermissionService,
			IEmployeeService employeeService,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			INomenclatureSelectorFactory nomenclatureSelectorFactory,
			IWarehouseRepository warehouseRepository,
			IUserRepository userRepository,
			IRDLPreviewOpener rdlPreviewOpener,
			ICommonServices commonServices) 
		: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.entityExtendedPermissionValidator = entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			warehousePermissionValidator = warehousePermissionService.GetValidator(CommonServices.UserService.CurrentUserId);

			canEditRectroactively = entityExtendedPermissionValidator.Validate(typeof(MovementDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			ConfigureEntityChangingRelations();
			if(UoW.IsNew) {
				Entity.DocumentType = MovementDocumentType.Transportation;
				SetDefaultWarehouseFrom();
			}
		}

		private bool canEditRectroactively;

		public bool CanEdit => PermissionResult.CanUpdate && (Entity.TimeStamp.Date == DateTime.Today || canEditRectroactively);

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.CanSend, () => CanSend);
			SetPropertyChangeRelation(e => e.CanReceive, () => CanReceive);
			SetPropertyChangeRelation(e => e.CanAcceptDiscrepancy, () => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(e => e.Status, () => CanEditNewDocument);
			SetPropertyChangeRelation(e => e.ToWarehouse, () => CanSend, () => CanReceive, () => CanAcceptDiscrepancy);
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSend, () => CanReceive, () => CanAcceptDiscrepancy);

			SetPropertyChangeRelation(e => e.CanAddItem, () => CanAddItem);
			SetPropertyChangeRelation(e => e.CanDeleteItems, () => CanDeleteItems);

			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSelectWarehouseTo);
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanAddItem);
			OnEntityPropertyChanged(ReloadAllowedWarehousesTo, e => e.FromWarehouse);

			Entity.ObservableItems.ElementAdded += (aList, aIdx) => OnPropertyChanged(nameof(CanChangeWarehouseFrom));
			Entity.ObservableItems.ElementRemoved += (aList, aIdx, aObject) => OnPropertyChanged(nameof(CanChangeWarehouseFrom));
		}

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
				if(Entity.Sender == null || Entity.SendTime == null ) {
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

		private void ReloadAllowedWarehousesFrom()
		{
			var allowedWarehouses = warehousePermissionValidator.GetAllowedWarehouses(WarehousePermissions.MovementEdit);
			allowedWarehousesFrom = UoW.Session.QueryOver<Warehouse>()
				.Where(x => !x.IsArchive)
				.WhereRestrictionOn(x => x.Id).Not.IsIn(allowedWarehouses.Select(x => x.Id).ToArray())
				.List();
			OnPropertyChanged(nameof(AllowedWarehousesFrom));
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
		}

		private IEnumerable<Warehouse> allowedWarehousesFrom;
		public IEnumerable<Warehouse> AllowedWarehousesFrom {
			get {
				if(allowedWarehousesFrom == null) {
					ReloadAllowedWarehousesFrom();
				}
				return allowedWarehousesFrom;
			}
		}

		private void SetDefaultWarehouseFrom()
		{
			if(CurrentUserSettings == null || CurrentUserSettings.DefaultWarehouse == null) {
				return;
			}

			Entity.FromWarehouse = CurrentUserSettings.DefaultWarehouse;
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

		public bool CanSelectWarehouseTo => Entity.FromWarehouse != null;

		public bool CanEditSendedAmount => CanSend;

		public bool CanEditReceivedAmount => CanReceive;

		public bool CanEditNewDocument => CanEdit && (Entity.Status == MovementDocumentStatus.New || Entity.Status == MovementDocumentStatus.Sended);

		public bool CanChangeWarehouseFrom => CanEditNewDocument && !Entity.Items.Any();

		#region Commands

		public bool CanSend => CanEdit
			&& Entity.CanSend
			&& warehousePermissionValidator.Validate(WarehousePermissions.MovementEdit, Entity.FromWarehouse);

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
			&& warehousePermissionValidator.Validate(WarehousePermissions.MovementEdit, Entity.ToWarehouse);

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
			&& warehousePermissionValidator.Validate(WarehousePermissions.MovementEdit, Entity.FromWarehouse);

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
								if(!e.SelectedNodes.Any()) {
									return;
								}
								var selectedNomenclaturesIds = e.SelectedNodes.Select(x => x.Id);
								var selectedNomenclatures =  UoW.GetById<Nomenclature>(selectedNomenclaturesIds);

								var nomenclaturesStock = warehouseRepository.GetWarehouseNomenclatureStock(UoW, Entity.FromWarehouse.Id, selectedNomenclaturesIds);

								foreach(var nomenclature in selectedNomenclatures) {
									var foundStockInfo = nomenclaturesStock.FirstOrDefault(x => x.NomenclatureId == nomenclature.Id);
									decimal stock = foundStockInfo?.Stock ?? 0;
									Entity.AddItem(nomenclature, 0, stock);
								}
								OnPropertyChanged(nameof(CanSend));
								OnPropertyChanged(nameof(CanReceive));
								OnPropertyChanged(nameof(CanAcceptDiscrepancy));
							};
							TabParent.OpenTab(() => nomenclatureSelector, this);

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

		private DelegateCommand printCommand;
		public DelegateCommand PrintCommand {
			get {
				if(printCommand == null) {
					printCommand = new DelegateCommand(
						() => {
							if(Entity.Status == MovementDocumentStatus.New && SendCommand.CanExecute()) {
								if(CommonServices.InteractiveService.InteractiveQuestion.Question("Перед печать необходимо отправить перемещение. Отправить?", "Печать документа перемещения")) {
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
