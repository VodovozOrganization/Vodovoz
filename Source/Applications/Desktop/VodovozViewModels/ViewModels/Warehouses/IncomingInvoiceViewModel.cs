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
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Models;
using Vodovoz.PermissionExtensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Warehouses
{
	public class IncomingInvoiceViewModel: EntityTabViewModelBase<IncomingInvoice>
	{
		private readonly IEmployeeService employeeService;
		private readonly INomenclatureJournalFactory nomenclatureSelectorFactory;
		private readonly IOrderSelectorFactory orderSelectorFactory;
		private readonly IRDLPreviewOpener rdlPreviewOpener;
		private readonly INomenclaturePurchasePriceModel _nomenclaturePurchasePriceModel;
		private readonly IWarehousePermissionValidator warehousePermissionValidator;
		private readonly IStockRepository _stockRepository;
		
		#region Конструктор
		public IncomingInvoiceViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehousePermissionService warehousePermissionService,
			IEmployeeService employeeService,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			IOrderSelectorFactory orderSelectorFactory,
			IWarehouseRepository warehouseRepository,
			IRDLPreviewOpener rdlPreviewOpener,
			ICommonServices commonServices,
			INomenclaturePurchasePriceModel nomenclaturePurchasePriceModel,
			IStockRepository stockRepository) 
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			
			if(warehouseRepository == null)
			{
				throw new ArgumentNullException(nameof(warehouseRepository));
			}

			this.rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_nomenclaturePurchasePriceModel = nomenclaturePurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclaturePurchasePriceModel));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			warehousePermissionValidator = warehousePermissionService.GetValidator();

			canEditRectroactively =
				(entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator)))
				.Validate(typeof(MovementDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			ConfigureEntityChangingRelations();
			
			ValidationContext.ServiceContainer.AddService(typeof(IWarehouseRepository), warehouseRepository);
			UserHasOnlyAccessToWarehouseAndComplaints =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !CurrentUser.IsAdmin;
		}
		#endregion

		#region Functions

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.CanAddItem, () => CanAddItem, () => CanFillFromOrders);
			SetPropertyChangeRelation(e => e.CanDeleteItems, () => CanDeleteItems);
			
			SetPropertyChangeRelation(e => e.Warehouse, () => CanAddItem, () => CanFillFromOrders);
			SetPropertyChangeRelation(e => e.Warehouse, () => CanCreate);
		}
		
		private void ReloadAllowedWarehousesFrom()
		{
			var allowedWarehouses =
				warehousePermissionValidator.GetAllowedWarehouses(
					isNew ? WarehousePermissionsType.IncomingInvoiceCreate : WarehousePermissionsType.IncomingInvoiceEdit, CurrentEmployee);
			allowedWarehousesFrom = UoW.Session.QueryOver<Warehouse>()
				.Where(x => !x.IsArchive)
				.WhereRestrictionOn(x => x.Id).IsIn(allowedWarehouses.Select(x => x.Id).ToArray())
				.List();
			OnPropertyChanged(nameof(AllowedWarehousesFrom));
			OnPropertyChanged(nameof(Warehouses));
		}
		
		public override bool Save(bool close)
		{
			if(!CanEdit)
				return false;
			
			if(UoW.IsNew) {
				Entity.Author = CurrentEmployee;
				Entity.TimeStamp = DateTime.Now;
			}
			else
			{
				if(Entity.LastEditor == null) {
					throw new InvalidOperationException("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				}
			}

			CreatePurchasePrices();

			Entity.LastEditor = CurrentEmployee;
			Entity.LastEditedTime = DateTime.Now;
			

			return base.Save(close);
		}

		private void CreatePurchasePrices()
		{
			foreach(var item in Entity.Items)
			{
				if(item.Nomenclature.UsingInGroupPriceSet)
				{
					continue;
				}

				var canCreateNewPrice = _nomenclaturePurchasePriceModel.CanCreatePrice(item.Nomenclature, Entity.TimeStamp.Date, item.PrimeCost);
				if(!canCreateNewPrice)
				{
					continue;
				}
				var newPrice = _nomenclaturePurchasePriceModel.CreatePrice(item.Nomenclature, Entity.TimeStamp.Date, item.PrimeCost);
				UoW.Save(newPrice);
			}
		}

		#endregion

		#region Properties
		
		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; }

		public bool isNew => Entity.Id == 0;

		private readonly bool canEditRectroactively;
		public bool CanEdit => 
			(UoW.IsNew && PermissionResult.CanCreate) 
			|| (PermissionResult.CanUpdate)
			|| canEditRectroactively;

		public bool CanAddItem => CanEdit && Entity.CanAddItem && Entity.Warehouse != null;
		public bool CanDeleteItems => CanEdit && Entity.CanDeleteItems;
		public bool CanFillFromOrders => CanEdit && Entity.CanAddItem && Entity.Warehouse != null;

		public string TotalSum
		{
			get
			{
				return $"Итого: {Entity.TotalSum:N2} ₽";
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
		
		public IEnumerable<Warehouse> Warehouses {
			get {
				var result = new List<Warehouse>(AllowedWarehousesFrom);
				if(Entity.Warehouse != null && !AllowedWarehousesFrom.Contains(Entity.Warehouse)) {
					result.Add(Entity.Warehouse);
				}
				return result;
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
		private bool DocumentHasChanges { get { return UoWGeneric.HasChanges; } }
		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;
		
		#endregion
		
		#region Commands

		private DelegateCommand<IncomingInvoiceItem> deleteItemCommand;
		public DelegateCommand<IncomingInvoiceItem> DeleteItemCommand {
			get {
				if(deleteItemCommand == null) {
					deleteItemCommand = new DelegateCommand<IncomingInvoiceItem>(
						(selectedItem) => {
							Entity.DeleteItem(selectedItem);
							OnPropertyChanged(nameof(TotalSum));
						},
						(selectedItem) =>  CanDeleteItems && selectedItem != null
					);
					deleteItemCommand.CanExecuteChangedWith(this, x => x.CanDeleteItems);
				}
				return deleteItemCommand;
			}
		}
		
		private DelegateCommand addItemCommand;
		public DelegateCommand AddItemCommand {
			get {
				if(addItemCommand == null) {
					addItemCommand = new DelegateCommand(
						() => {
							
							var alreadyAddedNomenclatures = Entity.Items
								.Where(x => x.Nomenclature != null)
								.Select(x => x.Nomenclature.Id);
							
							
							var nomenclatureSelector = nomenclatureSelectorFactory
								.CreateNomenclatureSelector(alreadyAddedNomenclatures);
							
							nomenclatureSelector.OnEntitySelectedResult += (sender, e) => {
								var selectedNodes = e.SelectedNodes;
								if(!selectedNodes.Any()) {
									return;
								}
								
								var selectedNomenclatures = UoW.GetById<Nomenclature>(
									selectedNodes.Select(x => x.Id)
								);
								
								foreach(var nomenclature in selectedNomenclatures) {
									Entity.AddItem(new IncomingInvoiceItem(){Nomenclature = nomenclature, Amount = 1});
									OnPropertyChanged(nameof(TotalSum));
								}
								
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
								if (nomIds != null && nomIds.Any()) 
								{
									nomIds = nomIds.Distinct().ToList();
									nomsAmount = _stockRepository.NomenclatureInStock(UoW, nomIds.ToArray(), Entity.Warehouse.Id);
								}
								//Если такие уже добавлены, то только увеличить их количество
								foreach (var item in orderItems) {
									var moveItem = Entity.Items.FirstOrDefault(x => x.Nomenclature.Id == item.Nomenclature.Id);
									if (moveItem == null)
									{
										var count = item.Count > nomsAmount[item.Nomenclature.Id]
											? nomsAmount[item.Nomenclature.Id]
											: item.Count;
										if ((count == 0) && item.Nomenclature.OnlineStore == null)
											continue;
										
										if (item.Nomenclature.Category == NomenclatureCategory.service || item.Nomenclature.Category == NomenclatureCategory.master)
											continue;
									
										Entity.AddItem( new IncomingInvoiceItem(){Nomenclature = item.Nomenclature, Amount = item.Count, PrimeCost = item.Price} );
									} else {
										var count = (moveItem.Amount + item.Count) > nomsAmount[item.Nomenclature.Id] ?
											nomsAmount[item.Nomenclature.Id] :
											(moveItem.Amount + item.Count);
										if(count == 0)
											continue;
										moveItem.Amount = count;
									}
								}
								
								OnPropertyChanged(nameof(TotalSum));
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
							int? currentEmployeeId = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId)?.Id;
							var doc = new IncomingInvoiceDocumentRDL(Entity, currentEmployeeId){Title = Entity.Title};
							if(doc is IPrintableRDLDocument) {
								rdlPreviewOpener.OpenRldDocument(typeof(IncomingInvoice), doc);
							}
						},
						() => true
					);
					
				}
				return printCommand;
			}
		}
		

		#endregion
	   
	}
}
