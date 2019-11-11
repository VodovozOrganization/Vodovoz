using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Documents;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Domain.Employees;
using QS.DomainModel.Entity.PresetPermissions;
using Vodovoz.Infrastructure.Permissions;
using System.Collections.Generic;
using Vodovoz.Domain.Store;
using System.Linq;
using Vodovoz.EntityRepositories;
using Vodovoz.TempAdapters;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Store;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using Vodovoz.PermissionExtensions;

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
			ICommonServices commonServices) 
		: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.entityExtendedPermissionValidator = entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			warehousePermissionValidator = warehousePermissionService.GetValidator(CommonServices.UserService.CurrentUserId);

			canEditRectroactively = entityExtendedPermissionValidator.Validate(typeof(MovementDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));

			if(UoW.IsNew) {
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
			SetPropertyChangeRelation(e => e.CanAddItem, () => CanAddItem);
			SetPropertyChangeRelation(e => e.CanDeleteItems, () => CanDeleteItems);
			SetPropertyChangeRelation(e => e.FromWarehouse, () => CanSelectWarehouseTo);
			OnEntityPropertyChanged(, e => e.FromWarehouse);

			//SetPropertyChangeRelation(e => e.Status, () => SendVisible, () => ReceiveVisible, () => AcceptDiscrepancyVisible);
		}

		private void ReloadAllowedWarehousesFrom()
		{
			var allowedWarehouses = warehousePermissionValidator.GetAllowedWarehouses(WarehousePermissions.MovementEdit);
			allowedWarehousesFrom = UoW.GetById<Warehouse>(allowedWarehouses.Select(x => x.Id));
		}

		private void ReloadAllowedWarehousesTo()
		{
			var allowedWarehouses = UoW.GetAll<Warehouse>().ToList();
			if(allowedWarehouses.Contains(Entity.FromWarehouse)) {
				allowedWarehouses.Remove(Entity.FromWarehouse);
			}
			if(Entity.) {

			}
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
					allowedWarehousesTo = UoW.GetAll<Warehouse>();
				}
				return allowedWarehousesTo;
			}
		}

		public bool CanChangeAlreadyDeliveredAmount => CommonServices.PermissionService.ValidateUserPresetPermission("can_change_delivered_movement_document_amount", CommonServices.UserService.CurrentUserId);

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

		/*public bool SendVisible {
			get {
				if(Entity.Status == MovementDocumentStatus.New || Entity.Status == MovementDocumentStatus.Sended) {
					return true;
				}
				return ;
			}
		}*/

		//public bool ReceiveVisible => Entity.Status == MovementDocumentStatus.Sended || (Entity.Status == MovementDocumentStatus.Accepted && );
		//public bool AcceptDiscrepancyVisible => Entity.Status == MovementDocumentStatus.New || Entity.Status == MovementDocumentStatus.Sended;

		public bool CanSelectWarehouseTo => Entity.FromWarehouse != null;

		public bool CanEditSendedAmount => CanSend;

		public bool CanEditReceivedAmount => CanReceive;

		#region Commands

		public bool CanSend => CanEdit
			&& Entity.CanSend
			&& warehousePermissionValidator.Validate(WarehousePermissions.MovementEdit, Entity.FromWarehouse);

		private DelegateCommand sendCommand;
		public DelegateCommand SendCommand {
			get {
				if(sendCommand == null) {
					sendCommand = new DelegateCommand(
						() => Entity.Send(CurrentEmployee),
						() => CanSend
					);
					sendCommand.CanExecuteChangedWith(this, x => x.CanSend);
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
						() => Entity.Receive(CurrentEmployee), 
						() => CanReceive
					);
					receiveCommand.CanExecuteChangedWith(this, x => x.CanReceive);
				}
				return receiveCommand;
			}
		}

		public bool CanAcceptDiscrepancy => CanEdit
			&& Entity.CanAcceptDiscrepancy
			&& warehousePermissionValidator.Validate(WarehousePermissions.MovementEdit, Entity.FromWarehouse);

		private DelegateCommand acceptDiscrepancyCommand;
		public DelegateCommand AcceptDiscrepancyCommand {
			get {
				if(acceptDiscrepancyCommand == null) {
					acceptDiscrepancyCommand = new DelegateCommand(
						() => Entity.AcceptDiscrepancy(CurrentEmployee),
						() => CanAcceptDiscrepancy
					);
					acceptDiscrepancyCommand.CanExecuteChangedWith(this, x => x.CanAcceptDiscrepancy);
				}
				return acceptDiscrepancyCommand;
			}
		}

		public bool CanAddItem => Entity.CanAddItem && Entity.FromWarehouse != null;

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
						(selectedItem) => Entity.DeleteItem(selectedItem),
						(selectedItem) => CanDeleteItems && selectedItem != null
					);
					deleteItemCommand.CanExecuteChangedWith(this, x => x.CanDeleteItems);
				}
				return deleteItemCommand;
			}
		}


		#endregion Commands
	}
}
