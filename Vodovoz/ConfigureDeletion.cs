using System.Collections.Generic;
using QSBanks;
using QSBusinessCommon.Domain;
using QSContacts;
using QSOrmProject.Deletion;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
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
				.AddDeleteDependence<WarehouseMovementOperation> (item => item.Nomenclature)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.Nomenclature)
				.AddDeleteDependence<IncomingInvoiceItem> (item => item.Nomenclature)
				.AddDeleteDependence<IncomingWater>(x => x.Product)
				.AddDeleteDependence<IncomingWaterMaterial>(x => x.Nomenclature)
				.AddDeleteDependence<MovementDocumentItem>(x => x.Nomenclature)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.Nomenclature)
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
				.AddDeleteDependence<PaidRentEquipment> (item => item.Equipment)
				.AddDeleteDependence<WarehouseMovementOperation> (item => item.Equipment)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.Equipment)
				.AddDeleteDependence<MovementDocumentItem>(x => x.Equipment)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.Equipment);

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
				.AddClearDependence<Car> (item => item.Driver)
				.AddClearDependence<Counterparty> (item => item.Accountant)
				.AddClearDependence<Counterparty> (item => item.SalesManager)
				.AddClearDependence<Counterparty> (item => item.BottlesManager)
				.AddClearDependence<MovementDocument> (item => item.ResponsiblePerson)
				.AddClearDependence<WriteoffDocument> (item => item.ResponsibleEmployee)
				.AddClearDependence<Organization> (item => item.Leader)
				.AddClearDependence<Organization> (item => item.Buhgalter)
				.AddClearDependence<Income> (item => item.Employee)
				.AddClearDependence<Expense> (item => item.Employee)
				.AddClearDependence<AccountExpense> (item => item.Employee)
				.AddClearDependence<CarLoadDocument> (item => item.Storekeeper)
				.AddClearDependence<CarUnloadDocument>(x => x.Storekeeper);

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

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(User),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Employee> (item => item.User)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<FreeRentPackage>()
				.AddClearDependence<FreeRentEquipment>(x => x.FreeRentPackage);

			DeleteConfig.AddHibernateDeleteInfo<PaidRentPackage>()
				.AddClearDependence<PaidRentEquipment>(x => x.PaidRentPackage);

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
				.AddDeleteDependence<WriteoffDocument> (item => item.Client)
				.AddDeleteDependence<AccountIncome> (item => item.Counterparty)
				.AddDeleteDependence<AccountExpense> (item => item.Counterparty)
				.AddDeleteDependence<MovementDocument> (item => item.FromClient)
				.AddDeleteDependence<MovementDocument> (item => item.ToClient)
				.AddClearDependence<Counterparty> (item => item.MainCounterparty);

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

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(CounterpartyContract),
				SqlSelect = "SELECT id, issue_date FROM @tablename ",
				DisplayString = "Договор №{0} от {1:d}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.CreateFromBag<CounterpartyContract> (item => item.AdditionalAgreements)
                                .AddCheckProperty<AdditionalAgreement> (item => item.Contract),
					DeleteDependenceInfo.Create<OrderContract>(x => x.Contract)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(AdditionalAgreement),
				SqlSelect = "SELECT id, number FROM @tablename ",
				DisplayString = "Доп. соглашение №{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<OrderItem> (item => item.AdditionalAgreement)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<WaterSalesAgreement>();

			DeleteConfig.AddHibernateDeleteInfo<RepairAgreement>();

			DeleteConfig.AddHibernateDeleteInfo<NonfreeRentAgreement>()
				.AddDeleteDependenceFromBag(x => x.Equipment);

			DeleteConfig.AddHibernateDeleteInfo<FreeRentAgreement>()
				.AddDeleteDependenceFromBag(x => x.Equipment);

			DeleteConfig.AddHibernateDeleteInfo<DailyRentAgreement>()
				.AddDeleteDependenceFromBag(x => x.Equipment);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(FreeRentEquipment),
				SqlSelect = "SELECT @tablename.id, model, serial_number FROM @tablename " +
				"LEFT JOIN equipment ON equipment.id = @tablename.equipment_id " +
				"LEFT JOIN nomenclature ON equipment.nomenclature_id = nomenclature.id ",
				DisplayString = "Доп. соглашение №{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<OrderItem> (item => item.AdditionalAgreement)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<PaidRentEquipment>();
				
			DeleteConfig.AddHibernateDeleteInfo<DeliveryPoint>()
				.AddDeleteDependence<AdditionalAgreement> (item => item.DeliveryPoint)
				.AddDeleteDependence<BottlesMovementOperation> (item => item.DeliveryPoint)
				.AddDeleteDependence<DepositOperation> (item => item.DeliveryPoint)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.WriteoffDeliveryPoint)
				.AddDeleteDependence<CounterpartyMovementOperation> (item => item.IncomingDeliveryPoint)
				.AddDeleteDependence<Proxy> (item => item.DeliveryPoint)
				.AddDeleteDependence<MovementDocument>(x => x.FromDeliveryPoint)
				.AddDeleteDependence<MovementDocument>(x => x.ToDeliveryPoint)
				.AddDeleteDependence<WriteoffDocument>(x => x.DeliveryPoint);
			
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
				.AddDeleteDependence<CarLoadDocument>(x => x.RouteList)
				.AddDeleteDependence<CarUnloadDocument>(x => x.RouteList);

			DeleteConfig.AddHibernateDeleteInfo<RouteColumn>()
				.AddClearDependence<Nomenclature>(x => x.RouteListColumn);

			#endregion

			//Вокруг заказа
			#region Order

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Order),
				SqlSelect = "SELECT id FROM @tablename ",
				DisplayString = "Заказ №{0}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<OrderItem> (item => item.Order),
					DeleteDependenceInfo.Create<OrderDocument> (item => item.Order),
					DeleteDependenceInfo.Create<OrderDepositItem> (item => item.Order),
					DeleteDependenceInfo.Create<CarLoadDocument> (item => item.Order)
				}
			}.FillFromMetaInfo ()
			);

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

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(OrderDocument),
				SqlSelect = "SELECT id, order_id FROM @tablename ",
				DisplayString = "Документ к заказу №{1}"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(OrderDepositItem),
				SqlSelect = "SELECT id, order_id FROM @tablename ",
				DisplayString = "Залог к заказу №{1}"
			}.FillFromMetaInfo ()
			);
				
			DeleteConfig.AddHibernateDeleteInfo<CommentTemplate>();

			DeleteConfig.AddHibernateDeleteInfo<ServiceClaim>()
				.AddDeleteDependence<DoneWorkDocument>(x => x.ServiceClaim)
				.AddDeleteDependence<EquipmentTransferDocument>(x => x.ServiceClaim);

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
				.AddClearDependence<Nomenclature>(x => x.Warehouse);

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
				.AddDeleteCascadeDependence(x => x.CounterpartyMovementOperation);

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
			
			#endregion

			//Операции в журналах
			#region Operations

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(BottlesMovementOperation),
				SqlSelect = "SELECT id, moved_to, moved_from FROM @tablename ",
				DisplayString = "Движения тары к контрагенту {1} от контрагента {2} бутылей"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddHibernateDeleteInfo<WarehouseMovementOperation>()
				.AddDeleteDependence<CarLoadDocumentItem>(x => x.MovementOperation)
				.AddDeleteDependence<CarUnloadDocumentItem>(x => x.MovementOperation)
				.AddDeleteDependence<IncomingInvoiceItem>(x => x.IncomeGoodsOperation)
				.AddDeleteDependence<IncomingWater>(x => x.ProduceOperation)
				.AddDeleteDependence<IncomingWaterMaterial>(x => x.ConsumptionMaterialOperation)
				.AddDeleteDependence<MovementDocumentItem>(x => x.WarehouseMovementOperation)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.WarehouseWriteoffOperation);

			DeleteConfig.AddHibernateDeleteInfo<CounterpartyMovementOperation>()
				.AddDeleteDependence<MovementDocumentItem>(x => x.CounterpartyMovementOperation)
				.AddDeleteDependence<WriteoffDocumentItem>(x => x.CounterpartyWriteoffOperation);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(MoneyMovementOperation),
				SqlSelect = "SELECT id FROM @tablename ",
				//FIXME Создать грамотную строку отобржения.
				DisplayString = "Денежная операция {0}"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(DepositOperation),
				SqlSelect = "SELECT id, received_deposit, refund_deposit FROM @tablename ",
				DisplayString = "Залог: получено = {1:C}, возврат = {2:C}"
			}.FillFromMetaInfo ()
			);

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
