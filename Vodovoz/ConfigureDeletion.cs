using System.Collections.Generic;
using QSBanks;
using QSBusinessCommon.Domain;
using QSContacts;
using QSOrmProject.Deletion;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	partial class MainClass
	{
		public static void ConfigureDeletion ()
		{
			logger.Info ("Настройка параметров удаления...");

			QSContactsMain.ConfigureDeletion ();
			QSBanksMain.ConfigureDeletion ();

			#region Goods

			DeleteConfig.AddHibernateDeleteInfo<Nomenclature>()
				.AddDeleteDependenceFromBag (item => item.NomenclaturePrice)
				.AddDeleteDependence<Equipment> (item => item.Nomenclature)
				.AddDeleteDependence<OrderItem>(x => x.Nomenclature)
				.AddDeleteDependence<OrderEquipment>(x => x.NewEquipmentNomenclature)
				.AddDeleteDependence<ServiceClaim>(x => x.Nomenclature)
				.AddDeleteDependence<ServiceClaimItem>(x => x.Nomenclature)
				.AddDeleteDependence<WarehouseMovementOperation> (item => item.Nomenclature)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.Nomenclature)
				.AddDeleteDependence<IncomingInvoiceItem> (item => item.Nomenclature)
				.AddDeleteDependence<IncomingWater>(x => x.Product)
				.AddDeleteDependence<IncomingWaterMaterial>(x => x.Nomenclature)
				.AddDeleteDependence<MovementDocumentItem>(x => x.Nomenclature)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.Nomenclature)
				.AddDeleteDependence<InventoryDocumentItem>(x => x.Nomenclature)
				.AddDeleteDependence<RegradingOfGoodsDocumentItem>(x => x.NomenclatureOld)
				.AddDeleteDependence<RegradingOfGoodsDocumentItem>(x => x.NomenclatureNew)
				.AddDeleteDependence<ProductSpecificationMaterial>(x => x.Material)
				.AddDeleteDependence<ProductSpecification>(x => x.Product)
				.AddClearDependence<PaidRentPackage>(x => x.RentServiceDaily)
				.AddClearDependence<PaidRentPackage>(x => x.RentServiceMonthly)
				.AddClearDependence<PaidRentPackage>(x => x.DepositService)
				.AddClearDependence<FreeRentPackage>(x => x.DepositService);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(EquipmentColors),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Nomenclature> (item => item.Color)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(EquipmentType),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<FreeRentPackage> (item => item.EquipmentType),
					DeleteDependenceInfo.Create<Nomenclature> (item => item.Type),
					DeleteDependenceInfo.Create<PaidRentPackage> (item => item.EquipmentType)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<Equipment>()
				.AddDeleteDependence<FreeRentEquipment> (item => item.Equipment)
				.AddDeleteDependence<IncomingInvoiceItem> (item => item.Equipment)
				.AddDeleteDependence<OrderEquipment> (item => item.Equipment)
				.AddDeleteDependence<OrderItem> (item => item.Equipment)
				.AddDeleteDependence<ServiceClaim>(x => x.Equipment)
				.AddDeleteDependence<PaidRentEquipment> (item => item.Equipment)
				.AddDeleteDependence<WarehouseMovementOperation> (item => item.Equipment)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.Equipment)
				.AddDeleteDependence<MovementDocumentItem>(x => x.Equipment)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.Equipment)
				.AddClearDependence<ServiceClaim>(x => x.ReplacementEquipment);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Manufacturer),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Nomenclature> (item => item.Manufacturer)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(MeasurementUnits),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Nomenclature> (item => item.Unit)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(NomenclaturePrice),
				SqlSelect = "SELECT id, price, min_count FROM @tablename ",
				DisplayString = "{1:C} (от {2})"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<CullingCategory>()
				.AddClearDependence<WriteoffDocumentItem>(x => x.CullingCategory);
			
			#endregion

			//Наша организация
			#region Organization

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Organization),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.CreateFromBag<Organization> (item => item.Phones),
					DeleteDependenceInfo.CreateFromBag<Organization> (item => item.Accounts),
					DeleteDependenceInfo.Create<CounterpartyContract> (item => item.Organization),
					DeleteDependenceInfo.Create<AccountIncome> (item => item.Organization),
					DeleteDependenceInfo.Create<AccountExpense> (item => item.Organization)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddClearDependence<Account> (ClearDependenceInfo.Create<Organization> (item => item.DefaultAccount));

			DeleteConfig.AddHibernateDeleteInfo<FreeRentPackage>()
				.AddClearDependence<FreeRentEquipment>(x => x.FreeRentPackage);

			DeleteConfig.AddHibernateDeleteInfo<PaidRentPackage>()
				.AddClearDependence<PaidRentEquipment>(x => x.PaidRentPackage);

			#endregion

			#region Сотрудники

			DeleteConfig.AddHibernateDeleteInfo<Employee>()
				.AddDeleteDependenceFromBag (item => item.Phones)
				.AddDeleteDependenceFromBag(item => item.Accounts)
				.AddDeleteDependence<Income> (item => item.Casher)
				.AddDeleteDependence<Expense> (item => item.Casher)
				.AddDeleteDependence<AdvanceReport> (item => item.Casher)
				.AddDeleteDependence<AdvanceReport> (item => item.Accountable)
				.AddDeleteDependence<RouteList>(x => x.Driver)
				.AddDeleteDependence<RouteList>(x => x.Forwarder)
				.AddDeleteDependence<RouteList>(x => x.Logistican)
				.AddDeleteDependence<FineItem>(x => x.Employee)
				.AddClearDependence<Car> (item => item.Driver)
				.AddClearDependence<Counterparty> (item => item.Accountant)
				.AddClearDependence<Counterparty> (item => item.SalesManager)
				.AddClearDependence<Counterparty> (item => item.BottlesManager)
				.AddClearDependence<Order>(x => x.Author)
				.AddClearDependence<ServiceClaim>(x => x.Engineer)
				.AddClearDependence<ServiceClaimHistory>(x => x.Employee)
				.AddClearDependence<MovementDocument> (item => item.ResponsiblePerson)
				.AddClearDependence<WriteoffDocument> (item => item.ResponsibleEmployee)
				.AddClearDependence<Organization> (item => item.Leader)
				.AddClearDependence<Organization> (item => item.Buhgalter)
				.AddClearDependence<Income> (item => item.Employee)
				.AddClearDependence<Expense> (item => item.Employee)
				.AddClearDependence<AccountExpense> (item => item.Employee)
				.AddClearDependence<CarLoadDocument> (item => item.Author)
				.AddClearDependence<CarLoadDocument> (item => item.LastEditor)
				.AddClearDependence<CarUnloadDocument>(x => x.Author)
				.AddClearDependence<CarUnloadDocument>(x => x.LastEditor)
				.AddClearDependence<IncomingInvoice>(x => x.Author)
				.AddClearDependence<IncomingInvoice>(x => x.LastEditor)
				.AddClearDependence<IncomingWater>(x => x.Author)
				.AddClearDependence<IncomingWater>(x => x.LastEditor)
				.AddClearDependence<MovementDocument>(x => x.Author)
				.AddClearDependence<MovementDocument>(x => x.LastEditor)
				.AddClearDependence<WriteoffDocument>(x => x.Author)
				.AddClearDependence<WriteoffDocument>(x => x.LastEditor)
				.AddClearDependence<InventoryDocument>(x => x.Author)
				.AddClearDependence<InventoryDocument>(x => x.LastEditor)
				.AddClearDependence<RegradingOfGoodsDocument>(x => x.Author)
				.AddClearDependence<RegradingOfGoodsDocument>(x => x.LastEditor)
				.AddClearDependence<RouteListClosing>(x => x.Cashier);

			DeleteConfig.AddClearDependence<Account> (ClearDependenceInfo.Create<Employee> (item => item.DefaultAccount));

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Nationality),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Employee> (item => item.Nationality)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<User>()
				.AddDeleteDependence<UserSettings>(x => x.User)
				.AddClearDependence<Employee> (item => item.User);

			DeleteConfig.AddHibernateDeleteInfo<UserSettings>();

			DeleteConfig.AddHibernateDeleteInfo<Fine>()
				.AddDeleteDependence<FineItem>(x => x.Fine)
				.AddClearDependence<InventoryDocumentItem>(x => x.Fine)
				.AddClearDependence<WriteoffDocumentItem>(x => x.Fine);

			DeleteConfig.AddHibernateDeleteInfo<FineItem>();

			#endregion

			//Контрагент и все что сним связано
			#region NearCounterparty

			DeleteConfig.AddHibernateDeleteInfo<Counterparty>()
				.AddDeleteDependenceFromBag(item => item.Phones)
				.AddDeleteDependenceFromBag(item => item.Emails)
				.AddDeleteDependenceFromBag(item => item.Accounts)
				.AddDeleteDependence<DeliveryPoint>(item => item.Counterparty)
				.AddDeleteDependence<Proxy>(item => item.Counterparty)
				.AddDeleteDependence<Contact> (item => item.Counterparty)
				.AddDeleteDependence<CounterpartyContract> (item => item.Counterparty)
				.AddDeleteDependence<BottlesMovementOperation> (item => item.Counterparty)
				.AddDeleteDependence<DepositOperation> (item => item.Counterparty)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.WriteoffCounterparty)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.IncomingCounterparty)
				.AddDeleteDependence<IncomingInvoice> (item => item.Contractor)
				.AddDeleteDependence<MoneyMovementOperation> (item => item.Counterparty)
				.AddDeleteDependence<MovementDocument> (item => item.FromClient)
				.AddDeleteDependence<MovementDocument> (item => item.ToClient)
				.AddDeleteDependence<Order> (item => item.Client)
				.AddDeleteDependence<ServiceClaim>(x => x.Counterparty)
				.AddDeleteDependence<WriteoffDocument> (item => item.Client)
				.AddDeleteDependence<AccountIncome> (item => item.Counterparty)
				.AddDeleteDependence<AccountExpense> (item => item.Counterparty)
				.AddDeleteDependence<MovementDocument> (item => item.FromClient)
				.AddDeleteDependence<MovementDocument> (item => item.ToClient)
				.AddClearDependence<Counterparty> (item => item.MainCounterparty)
				.AddClearDependence<Equipment>(x => x.AssignedToClient);

			DeleteConfig.AddClearDependence<Account> (ClearDependenceInfo.Create<Counterparty> (item => item.DefaultAccount));

			DeleteConfig.AddHibernateDeleteInfo<Contact>()
				.AddDeleteDependenceFromBag(item => item.Emails)
				.AddDeleteDependenceFromBag(item => item.Phones)
				.AddClearDependence<Counterparty> (item => item.MainContact)
				.AddClearDependence<Counterparty> (item => item.FinancialContact)
				.AddClearDependence<DeliveryPoint> (item => item.Contact);

			DeleteConfig.AddClearDependence<Post> (ClearDependenceInfo.Create<Contact> (item => item.Post));

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Significance),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Counterparty> (item => item.Significance)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(CounterpartyStatus),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Counterparty> (item => item.Status)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Proxy),
				SqlSelect = "SELECT id, number, issue_date FROM @tablename ",
				DisplayString = "{1} от {2:d}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.CreateFromBag<Proxy> (item => item.Persons)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<CounterpartyContract>()
				.AddDeleteDependence<AdditionalAgreement> (item => item.Contract)
				.AddDeleteDependence<OrderContract>(x => x.Contract)
				.AddClearDependence<Order>(x => x.Contract);

			DeleteConfig.AddHibernateDeleteInfo<AdditionalAgreement>().HasSubclasses()
				.AddDeleteDependence<OrderAgreement>(x => x.AdditionalAgreement)
				.AddClearDependence<OrderItem> (item => item.AdditionalAgreement);

			DeleteConfig.AddHibernateDeleteInfo<WaterSalesAgreement>();

			DeleteConfig.AddHibernateDeleteInfo<RepairAgreement>();

			DeleteConfig.AddHibernateDeleteInfo<NonfreeRentAgreement>()
				.AddDeleteDependenceFromBag(x => x.Equipment);

			DeleteConfig.AddHibernateDeleteInfo<FreeRentAgreement>()
				.AddDeleteDependenceFromBag(x => x.Equipment);

			DeleteConfig.AddHibernateDeleteInfo<DailyRentAgreement>()
				.AddDeleteDependenceFromBag(x => x.Equipment);

			DeleteConfig.AddHibernateDeleteInfo<FreeRentEquipment>()
				.AddClearDependence<OrderDepositItem>(x => x.FreeRentItem);

			DeleteConfig.AddHibernateDeleteInfo<PaidRentEquipment>()
				.AddClearDependence<OrderDepositItem>(x => x.PaidRentItem);
				
			DeleteConfig.AddHibernateDeleteInfo<DeliveryPoint>()
				.AddDeleteDependence<AdditionalAgreement> (item => item.DeliveryPoint)
				.AddDeleteDependence<BottlesMovementOperation> (item => item.DeliveryPoint)
				.AddDeleteDependence<DepositOperation> (item => item.DeliveryPoint)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.WriteoffDeliveryPoint)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.IncomingDeliveryPoint)
				.AddDeleteDependence<Proxy> (item => item.DeliveryPoint)
				.AddDeleteDependence<Order>(x => x.DeliveryPoint)
				.AddDeleteDependence<MovementDocument>(x => x.FromDeliveryPoint)
				.AddDeleteDependence<MovementDocument>(x => x.ToDeliveryPoint)
				.AddDeleteDependence<WriteoffDocument>(x => x.DeliveryPoint)
				.AddClearDependence<ServiceClaim>(x => x.DeliveryPoint);
			
			#endregion
				
			#region Logistics

			DeleteConfig.AddHibernateDeleteInfo<Car>()
				.AddDeleteDependence<RouteList>(x => x.Car);

			DeleteConfig.AddHibernateDeleteInfo<FuelType>()
				.AddClearDependence<Car>(x => x.FuelType);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(DeliverySchedule),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<Order> (item => item.DeliverySchedule)	
				},
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<DeliveryPoint> (item => item.DeliverySchedule)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<DeliveryShift>()
				.AddClearDependence<RouteList>(x => x.Shift);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(LogisticsArea),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<DeliveryPoint> (item => item.LogisticsArea)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<RouteList>()
				.AddDeleteDependence<RouteListItem>(x => x.RouteList)
				.AddDeleteDependence<CarLoadDocument>(x => x.RouteList)
				.AddDeleteDependence<CarUnloadDocument>(x => x.RouteList)
				.AddDeleteDependence<RouteListClosing>(x => x.RouteList);

			DeleteConfig.AddHibernateDeleteInfo<RouteListClosing>()
				.AddClearDependence<Expense>(x => x.RouteListClosing)
				.AddClearDependence<Income>(x => x.RouteListClosing);

			DeleteConfig.AddHibernateDeleteInfo<RouteColumn>()
				.AddClearDependence<Nomenclature>(x => x.RouteListColumn);

			DeleteConfig.AddHibernateDeleteInfo<RouteListItem>()
				.AddRemoveFromDependence<RouteList>(x => x.Addresses, x => x.RemoveAddress);

			#endregion

			//Вокруг заказа
			#region Order

			DeleteConfig.AddHibernateDeleteInfo<Order>()
				.AddDeleteDependence<OrderItem> (item => item.Order)
				.AddDeleteDependence<OrderEquipment>(x => x.Order)
				.AddDeleteDependence<OrderDocument> (item => item.Order)
				.AddDeleteDependence<OrderDepositItem> (item => item.Order)
				.AddDeleteDependence<RouteListItem>(x => x.Order)
				.AddDeleteDependence<CarLoadDocument> (item => item.Order)
				.AddDeleteDependence<BottlesMovementOperation>(item => item.Order)
				.AddDeleteDependence<DepositOperation>(x => x.Order)
				.AddDeleteDependence<MoneyMovementOperation>(x => x.Order)
				.AddDeleteDependence<OrderDocument>(x => x.AttachedToOrder)
				.AddDeleteCascadeDependence(x => x.BottlesMovementOperation)
				.AddDeleteCascadeDependence(x => x.MoneyMovementOperation)
				.AddClearDependence<Order>(x => x.PreviousOrder)
				.AddClearDependence<ServiceClaim>(x => x.InitialOrder)
				.AddClearDependence<ServiceClaim>(x => x.FinalOrder);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(OrderItem),
				SqlSelect = "SELECT id, order_id FROM @tablename ",
				DisplayString = "Строка заказа №{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<OrderEquipment> (item => item.OrderItem)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(OrderEquipment),
				SqlSelect = "SELECT id, order_id FROM @tablename ",
				DisplayString = "Оборудование заказа №{1}"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<OrderDocument>().HasSubclasses();

			DeleteConfig.AddHibernateDeleteInfo<OrderDepositItem>()
				.AddDeleteCascadeDependence(x => x.DepositOperation);
				
			DeleteConfig.AddHibernateDeleteInfo<CommentTemplate>();

			DeleteConfig.AddHibernateDeleteInfo<ServiceClaim>()
				.AddDeleteDependence<ServiceClaimItem>(x => x.ServiceClaim)
				.AddDeleteDependence<ServiceClaimHistory>(x => x.ServiceClaim)
				.AddDeleteDependence<DoneWorkDocument>(x => x.ServiceClaim)
				.AddDeleteDependence<EquipmentTransferDocument>(x => x.ServiceClaim);

			DeleteConfig.AddHibernateDeleteInfo<ServiceClaimItem>();

			DeleteConfig.AddHibernateDeleteInfo<ServiceClaimHistory>();

			#endregion

			#region Документы заказа

			DeleteConfig.AddHibernateDeleteInfo<BillDocument>();

			DeleteConfig.AddHibernateDeleteInfo<CoolerWarrantyDocument>();

			DeleteConfig.AddHibernateDeleteInfo<DoneWorkDocument>();

			DeleteConfig.AddHibernateDeleteInfo<EquipmentTransferDocument>();

			DeleteConfig.AddHibernateDeleteInfo<InvoiceBarterDocument>();

			DeleteConfig.AddHibernateDeleteInfo<InvoiceDocument>();

			DeleteConfig.AddHibernateDeleteInfo<OrderAgreement>();

			DeleteConfig.AddHibernateDeleteInfo<OrderContract>();

			DeleteConfig.AddHibernateDeleteInfo<PumpWarrantyDocument>();

			DeleteConfig.AddHibernateDeleteInfo<UPDDocument>();

			DeleteConfig.AddHibernateDeleteInfo<DriverTicketDocument>();

			#endregion

			//Документы
			#region Склад

			DeleteConfig.AddHibernateDeleteInfo<Warehouse>()
				.AddDeleteDependence<IncomingInvoice> (item => item.Warehouse)
				.AddDeleteDependence<CarLoadDocument>(x => x.Warehouse)
				.AddDeleteDependence<CarUnloadDocument>(x => x.Warehouse)
				.AddDeleteDependence<IncomingWater>(x => x.IncomingWarehouse)
				.AddDeleteDependence<IncomingWater>(x => x.WriteOffWarehouse)
				.AddDeleteDependence<MovementDocument>(x => x.FromWarehouse)
				.AddDeleteDependence<MovementDocument>(x => x.ToWarehouse)
				.AddDeleteDependence<WarehouseMovementOperation>(x => x.IncomingWarehouse)
				.AddDeleteDependence<WarehouseMovementOperation>(x => x.WriteoffWarehouse)
				.AddDeleteDependence<WriteoffDocument>(x => x.WriteoffWarehouse)
				.AddDeleteDependence<InventoryDocument>(x => x.Warehouse)
				.AddDeleteDependence<RegradingOfGoodsDocument>(x => x.Warehouse)
				.AddClearDependence<Nomenclature>(x => x.Warehouse)
				.AddClearDependence<UserSettings>(x => x.DefaultWarehouse);

			DeleteConfig.AddHibernateDeleteInfo<IncomingInvoice>()
				.AddDeleteDependence<IncomingInvoiceItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<IncomingInvoiceItem>()
				.AddDeleteCascadeDependence(x => x.IncomeGoodsOperation);

			DeleteConfig.AddHibernateDeleteInfo<IncomingWater>()
				.AddDeleteDependence<IncomingWaterMaterial>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<IncomingWaterMaterial>()
				.AddDeleteCascadeDependence(x => x.ConsumptionMaterialOperation);

			DeleteConfig.AddHibernateDeleteInfo<MovementDocument>()
				.AddDeleteDependence<MovementDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<MovementDocumentItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseMovementOperation)
				.AddDeleteCascadeDependence(x => x.CounterpartyMovementOperation)
				.AddDeleteCascadeDependence(x => x.DeliveryMovementOperation);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffDocument>()
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<WriteoffDocumentItem>()
				.AddDeleteCascadeDependence(x => x.CounterpartyWriteoffOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseWriteoffOperation);

			DeleteConfig.AddHibernateDeleteInfo<ProductSpecification>()
				.AddDeleteDependenceFromBag(x => x.Materials);

			DeleteConfig.AddHibernateDeleteInfo<ProductSpecificationMaterial>();

			DeleteConfig.AddHibernateDeleteInfo<CarLoadDocument>()
				.AddDeleteDependence<CarLoadDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<CarLoadDocumentItem>()
				.AddDeleteCascadeDependence(x => x.MovementOperation);

			DeleteConfig.AddHibernateDeleteInfo<CarUnloadDocument>()
				.AddDeleteDependence<CarUnloadDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<CarUnloadDocumentItem>()
				.AddDeleteCascadeDependence(x => x.MovementOperation);

			DeleteConfig.AddHibernateDeleteInfo<InventoryDocument>()
				.AddDeleteDependence<InventoryDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<InventoryDocumentItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseChangeOperation);

			DeleteConfig.AddHibernateDeleteInfo<RegradingOfGoodsDocument>()
				.AddDeleteDependence<RegradingOfGoodsDocumentItem>(x => x.Document);

			DeleteConfig.AddHibernateDeleteInfo<RegradingOfGoodsDocumentItem>()
				.AddDeleteCascadeDependence(x => x.WarehouseIncomeOperation)
				.AddDeleteCascadeDependence(x => x.WarehouseWriteOffOperation);

			DeleteConfig.AddHibernateDeleteInfo<MovementWagon>()
				.AddClearDependence<MovementDocument>(x => x.MovementWagon);

			#endregion

			//Операции в журналах
			#region Operations

			DeleteConfig.AddHibernateDeleteInfo<BottlesMovementOperation>()
				.AddClearDependence<Order>(x => x.BottlesMovementOperation);

			DeleteConfig.AddHibernateDeleteInfo<WarehouseMovementOperation>()
				.AddDeleteDependence<CarLoadDocumentItem>(x => x.MovementOperation)
				.AddDeleteDependence<CarUnloadDocumentItem>(x => x.MovementOperation)
				.AddDeleteDependence<IncomingInvoiceItem>(x => x.IncomeGoodsOperation)
				.AddDeleteDependence<IncomingWater>(x => x.ProduceOperation)
				.AddDeleteDependence<IncomingWaterMaterial>(x => x.ConsumptionMaterialOperation)
				.AddDeleteDependence<MovementDocumentItem>(x => x.WarehouseMovementOperation)
				.AddDeleteDependence<MovementDocumentItem>(x => x.DeliveryMovementOperation)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.WarehouseWriteoffOperation)
				.AddDeleteDependence<InventoryDocumentItem>(x => x.WarehouseChangeOperation)
				.AddDeleteDependence<RegradingOfGoodsDocumentItem>(x => x.WarehouseIncomeOperation)
				.AddDeleteDependence<RegradingOfGoodsDocumentItem>(x => x.WarehouseWriteOffOperation);

			DeleteConfig.AddHibernateDeleteInfo<CounterpartyMovementOperation>()
				.AddDeleteDependence<MovementDocumentItem>(x => x.CounterpartyMovementOperation)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.CounterpartyWriteoffOperation);

			DeleteConfig.AddHibernateDeleteInfo<MoneyMovementOperation>()
				.AddClearDependence<Order>(x => x.MoneyMovementOperation);

			DeleteConfig.AddHibernateDeleteInfo<DepositOperation>()
				.AddDeleteDependence<OrderDepositItem>(x => x.DepositOperation);

			#endregion

			#region Cash

			DeleteConfig.AddHibernateDeleteInfo<Income>()
				.AddDeleteDependence<AdvanceClosing>(x => x.Income)
				.AddDeleteDependence<AdvanceReport>(x => x.ChangeReturn);

			DeleteConfig.AddHibernateDeleteInfo<Expense>()
				.AddDeleteDependence<AdvanceClosing>(x => x.AdvanceExpense);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(AdvanceReport),
				SqlSelect = "SELECT id, date FROM @tablename ",
				DisplayString = "Авансовый отчет №{0} от {1:d}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<AdvanceClosing> (item => item.AdvanceReport) //FIXME Запустить перерасчет калки закрытия. 
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo(new DeleteInfo {
				ObjectClass = typeof(AdvanceClosing),
				SqlSelect = "SELECT id FROM @tablename ",
				DisplayString = "Строка закрытия аванса №{0} на сумму #FIXME",
			}.FillFromMetaInfo ());

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(IncomeCategory),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "Статья дохода {1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<AccountIncome> (item => item.Category),
					DeleteDependenceInfo.Create<Income> (item => item.IncomeCategory)
				}
			}.FillFromMetaInfo ()
			);
				
			DeleteConfig.AddHibernateDeleteInfo<ExpenseCategory>()
				.AddDeleteDependence<Expense> (item => item.ExpenseCategory)
				.AddDeleteDependence<AdvanceReport> (item => item.ExpenseCategory)
				.AddDeleteDependence<Income> (item => item.ExpenseCategory)
				.AddDeleteDependence<AccountExpense> (item => item.Category)
				.AddDeleteDependence<ExpenseCategory>(x => x.Parent)
				.AddClearDependence<Counterparty> (item => item.DefaultExpenseCategory);

			#endregion

			#region Операции по счету

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(AccountIncome),
				SqlSelect = "SELECT id, number, date, total FROM @tablename ",
				DisplayString = "Операция прихода №{1} от {2:d} на сумму {3}₽"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(AccountExpense),
				SqlSelect = "SELECT id, number, date, total FROM @tablename ",
				DisplayString = "Операция расхода №{1} от {2:d} на сумму {3}₽"
			}.FillFromMetaInfo ()
			);
				
			DeleteConfig.ExistingConfig<Account> ().DeleteItems
				.AddRange (new List<DeleteDependenceInfo> {
				DeleteDependenceInfo.Create<AccountIncome> (item => item.CounterpartyAccount),
				DeleteDependenceInfo.Create<AccountIncome> (item => item.OrganizationAccount),
				DeleteDependenceInfo.Create<AccountExpense> (item => item.CounterpartyAccount),
				DeleteDependenceInfo.Create<AccountExpense> (item => item.OrganizationAccount),
				DeleteDependenceInfo.Create<AccountExpense> (item => item.EmployeeAccount),
			});

			#endregion

			//Для тетирования
			#if DEBUG
			//DeleteConfig.IgnoreMissingClass.Add (typeof(NonfreeRentAgreement));
			//DeleteConfig.IgnoreMissingClass.Add (typeof(DailyRentAgreement));
			//DeleteConfig.IgnoreMissingClass.Add (typeof(FreeRentAgreement));
			//DeleteConfig.IgnoreMissingClass.Add (typeof(WaterSalesAgreement));
			//DeleteConfig.IgnoreMissingClass.Add (typeof(RepairAgreement));

			DeleteConfig.DeletionCheck ();
			#endif

			logger.Info ("Ок");
		}
	}
}
