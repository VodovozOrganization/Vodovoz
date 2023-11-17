using Autofac;
using Dialogs.Employees;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Report.ViewModels;
using QSReport;
using System;
using Vodovoz;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.JournalViewModels.Suppliers;
using Vodovoz.Old1612ExportTo1c;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Reports;
using Vodovoz.Representations;
using Vodovoz.ServiceDialogs;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Cash.DocumentsJournal;
using Vodovoz.ViewModels.Cash.Transfer.Journal;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Logistic.DriversStopLists;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Action = Gtk.Action;

public partial class MainWindow : Window
{
	//Заказы
	Action ActionOrdersTable;
	Action ActionAddOrder;
	Action ActionLoadOrders;
	Action ActionDeliveryPrice;
	Action ActionUndeliveredOrders;
	Action ActionCashReceiptsJournal;
	Action ActionOrdersWithReceiptJournal;

	Action ActionServiceClaims;
	Action ActionWarehouseDocuments;
	Action ActionWarehouseStock;
	Action ActionClientBalance;
	Action ActionWarehouseDocumentsItemsJournal;

	//Работа с клиентами
	Action ActionCallTasks;
	Action ActionBottleDebtors;
	Action ActionIncomingCallsAnalysisReport;
	Action ActionRoboatsCallsRegistry;
	Action ActionDriversTareMessages;

	//Логистика
	Action ActionRouteListTable;
	Action ActionAtWorks;
	Action ActionRouteListsAtDay;
	Action ActionRouteListsPrint;
	Action ActionRouteListClosingTable;
	Action ActionRouteListMileageCheck;
	Action ActionRouteListTracking;
	Action ActionFastDeliveryAvailabilityJournal;
	Action ActionDriversStopLists;

	Action ActionReadyForShipment;
	Action ActionReadyForReception;
	Action ActionFinesJournal;
	Action ActionPremiumJournal;
	Action ActionCarProxiesJournal;
	Action ActionRevisionBottlesAndDeposits;
	Action ActionReportDebtorsBottles;
	Action ActionExportImportNomenclatureCatalog;

	//Бухгалтерия
	Action ActionTransferBankDocs;
	Action ActionPaymentFromBank;
	Action ActionAccountingTable;
	Action ActionAccountFlow;
	Action ActionRevision;
	Action ActionExportTo1c;
	Action ActionOldExportTo1c;
	Action ActionExportCounterpartiesTo1c;
	Action ActionImportPaymentsByCard;
	Action ActionFinancialDistrictsSetsJournal;
	Action ActionUnallocatedBalancesJournal;
	Action ActionImportPaymentsFromAvangard;

	Action ActionResidue;
	Action ActionEmployeeWorkChart;
	Action ActionRouteListAddressesTransferring;
	Action ActionTransferOperationJournal;
	Action ActionDistricts;

	//Касса
	Action ActionCashDocuments;
	Action ActionAccountableDebt;
	Action ActionUnclosedAdvances;
	Action ActionCashFlow;
	Action ActionSelfdeliveryOrders;
	Action ActionCashTransferDocuments;
	Action ActionFuelTransferDocuments;
	Action ActionOrganizationCashTransferDocuments;
	Action ActionSalariesJournal;

	//Suppliers
	Action ActionNewRequestToSupplier;
	Action ActionJournalOfRequestsToSuppliers;
	Action ActionWarehousesBalanceSummary;

	//ТрО
	private Action ActionCarEventsJournal;

	//Отдел продаж
	private Action ActionSalesOrdersJournal;
	private Action ActionSalesCounterpartiesJournal;
	private Action ActionSalesUndeliveredOrdersJournal;
	private Action ActionSalesComplaintsJournal;

	public void BuildToolbarActions()
	{
		#region Creating actions

		//Заказы
		ActionOrdersTable = new Action("ActionOrdersTable", "Журнал заказов", null, "table");
		ActionAddOrder = new Action("ActionAddOrder", "Новый заказ", null, "table");
		ActionLoadOrders = new Action("ActionLoadOrders", "Загрузить из 1С", null, "table");
		ActionDeliveryPrice = new Action("ActionDeliveryPrice", "Стоимость доставки", null, null);
		ActionUndeliveredOrders = new Action("ActionUndeliveredOrders", "Журнал недовозов", null, null);
		ActionCashReceiptsJournal = new Action(nameof(ActionCashReceiptsJournal), "Журнал чеков", null, "table");
		ActionOrdersWithReceiptJournal = new Action(nameof(ActionOrdersWithReceiptJournal), "Журнал заказов с чеками", null, "table");
		
		//Работа с клиентами
		ActionCallTasks = new Action("ActionCallTasks", "Журнал задач", null, "table");
		ActionBottleDebtors = new Action("ActionBottleDebtors", "Журнал задолженности", null, "table");
		ActionIncomingCallsAnalysisReport = new Action(nameof(ActionIncomingCallsAnalysisReport), "Анализ входящих звонков", null, "table");
		ActionRoboatsCallsRegistry = new Action(nameof(ActionRoboatsCallsRegistry), "Реестр звонков Roboats", null, "table");
		
		ActionDriversTareMessages = new Action(nameof(ActionDriversTareMessages), "Сообщения водителей по таре", null, "table");
		//Сервис
		ActionServiceClaims = new Action("ActionServiceTickets", "Журнал заявок", null, "table");

		//Склад
		ActionWarehouseDocuments = new Action("ActionWarehouseDocuments", "Журнал документов", null, "table");
		ActionReadyForShipment = new Action("ActionReadyForShipment", "Готовые к погрузке", null, "table");
		ActionReadyForReception = new Action("ActionReadyForReception", "Готовые к разгрузке", null, "table");
		ActionWarehouseStock = new Action("ActionWarehouseStock", "Складские остатки", null, "table");
		ActionClientBalance = new Action("ActionClientBalance", "Оборудование у клиентов", null, "table");
		ActionWarehouseDocumentsItemsJournal = new Action(nameof(ActionWarehouseDocumentsItemsJournal), "Журнал строк складских документов", null, "table");

		//Логистика
		ActionRouteListTable = new Action("ActionRouteListTable", "Журнал МЛ", null, "table");
		ActionAtWorks = new Action("ActionAtWorks", "На работе", null, "table");
		ActionRouteListsAtDay = new Action("ActionRouteListsAtDay", "Формирование МЛ", null, null);
		ActionRouteListsPrint = new Action("ActionRouteListsPrint", "Печать МЛ", null, "print");
		ActionRouteListClosingTable = new Action("ActionRouteListClosingTable", "Работа кассы с МЛ", null, "table");
		ActionRouteListTracking = new Action("ActionRouteListTracking", "Мониторинг машин", null, "table");
		ActionRouteListMileageCheck = new Action("ActionRouteListMileageCheck", "Контроль за километражем", null, "table");
		ActionRouteListAddressesTransferring = new Action("ActionRouteListAddressesTransferring", "Перенос адресов", null, "table");
		ActionFastDeliveryAvailabilityJournal = new Action("ActionFastDeliveryAvailabilityJournal", "Доставка за час", null, "table");
		ActionDriversStopLists = new Action("ActionDriversStopLists", "Стоп-лист", null, "table");
		//Касса
		ActionCashDocuments = new Action("ActionCashDocuments", "Кассовые документы", null, "table");
		ActionAccountableDebt = new Action("ActionAccountableDebt", "Долги сотрудников", null, "table");
		ActionUnclosedAdvances = new Action("ActionUnclosedAdvances", "Незакрытые авансы", null, "table");
		ActionCashFlow = new Action("ActionCashFlow", "Доходы и расходы", null, "table");
		ActionSelfdeliveryOrders = new Action("ActionSelfdeliveryOrders", "Журнал самовывозов", null, "table");
		ActionCashTransferDocuments = new Action("ActionCashTransferDocuments", "Журнал перемещения д/с", null, "table");
		ActionFuelTransferDocuments = new Action("ActionFuelTransferDocuments", "Журнал учета топлива", null, "table");
		ActionOrganizationCashTransferDocuments = new Action("ActionOrganizationCashTransferDocuments", "Журнал перемещения д/с для юр.лиц", null, "table");
		ActionSalariesJournal = new Action(nameof(ActionSalariesJournal), "Журнал выдач З/П", null, "table");

		//Бухгалтерия
		ActionTransferBankDocs = new Action("ActionTransferBankDocs", "Загрузка из банк-клиента", null, "table");
		ActionPaymentFromBank = new Action("ActionPaymentFromBank", "Загрузка выписки из банк-клиента", null, "table");
		ActionExportTo1c = new Action("ActionExportTo1c", "Выгрузка в 1с 8.3", null, "table");
		ActionOldExportTo1c = new Action("ActionOldExportTo1c", "Выгрузка в 1с 8.3 (до 16.12.2020)", null, "table");
		ActionExportCounterpartiesTo1c = new Action("ActionExportCounterpartiesTo1c", "Выгрузка контрагентов в 1с", null, "table");
		ActionImportPaymentsByCard = new Action("ActionImportPaymentsByCard", "Загрузка выписки оплат по карте", null, "table");
		ActionAccountingTable = new Action("ActionAccountingTable", "Операции по счету", null, "table");
		ActionAccountFlow = new Action("ActionAccountFlow", "Доходы и расходы (безнал)", null, "table");
		ActionRevision = new Action("ActionRevision", "Акт сверки", null, "table");
		ActionFinancialDistrictsSetsJournal = new Action("ActionFinancialDistrictsSetsJournal", "Версии финансовых районов", null, "table");
		ActionUnallocatedBalancesJournal = new Action("ActionUnallocatedBalancesJournal", "Журнал нераспределенных балансов", null, "table");
		ActionImportPaymentsFromAvangard = new Action("ActionImportPaymentsFromAvangard", "Загрузка реестра оплат из Авангарда", null, "table");

		//Архив
		ActionReportDebtorsBottles = new Action("ReportDebtorsBottles", "Отчет по должникам тары", null, "table");
		ActionRevisionBottlesAndDeposits = new Action("RevisionBottlesAndDeposits", "Акт по бутылям/залогам", null, "table");
		ActionResidue = new Action("ActionResidue", "Ввод остатков", null, "table");
		ActionTransferOperationJournal = new Action("ActionTransferOperationJournal", "Переносы между точками доставки", null, "table");

		//Кадры
		ActionEmployeeWorkChart = new Action("ActionEmployeeWorkChart", "График работы сотрудников", null, "table");
		ActionFinesJournal = new Action("ActionFinesJournal", "Штрафы", null, "table");
		ActionPremiumJournal = new Action("ActionPremiumJournal", "Премии", null, "table");
		ActionCarProxiesJournal = new Action("ActionCarProxiesJournal", "Журнал доверенностей", null, "table");
		ActionDistricts = new Action("ActionDistricts", "Версии районов", null, "table");

		//Suppliers
		ActionNewRequestToSupplier = new Action(nameof(ActionNewRequestToSupplier), "Новая заявка поставщику", null, "table");
		ActionJournalOfRequestsToSuppliers = new Action(nameof(ActionJournalOfRequestsToSuppliers), "Журнал заявок поставщику", null, "table");
		ActionExportImportNomenclatureCatalog = new Action("ActionExportImportNomenclatureCatalog", "Выгрузка/Загрузка каталога номенклатур", null, "table");
		ActionWarehousesBalanceSummary = new Action(nameof(ActionWarehousesBalanceSummary), "Остатки", null, "table");

		//ТрО
		ActionCarEventsJournal = new Action("ActionCarEventsJournal", "Журнал событий ТС", null, "table");

		//Отдел продаж
		ActionSalesOrdersJournal = new Action("ActionSalesOrdersJournal", "Журнал заказов", null, "table");
		ActionSalesCounterpartiesJournal = new Action("ActionSalesCounterpartiesJournal", "Журнал контрагентов", null, "table");
		ActionSalesUndeliveredOrdersJournal = new Action("ActionSalesUndeliveredOrdersJournal", "Журнал недовозов", null, "table");
		ActionSalesComplaintsJournal = new Action("ActionSalesComplaintsJournal", "Журнал рекламаций", null, "table");

		#endregion
		#region Inserting actions to the toolbar
		ActionGroup w1 = new ActionGroup("ToolbarActions");

		//Заказы
		w1.Add(ActionOrdersTable, null);
		w1.Add(ActionAddOrder, null);
		w1.Add(ActionLoadOrders, null);
		w1.Add(ActionDeliveryPrice, null);
		w1.Add(ActionUndeliveredOrders, null);
		w1.Add(ActionCashReceiptsJournal, null);
		w1.Add(ActionOrdersWithReceiptJournal, null);
		
		//
		w1.Add(ActionServiceClaims, null);
		w1.Add(ActionWarehouseDocuments, null);
		w1.Add(ActionReadyForShipment, null);
		w1.Add(ActionReadyForReception, null);
		w1.Add(ActionWarehouseStock, null);
		w1.Add(ActionClientBalance, null);
		w1.Add(ActionWarehouseDocumentsItemsJournal, null);

		//Работа с клиентами
		w1.Add(ActionCallTasks, null);
		w1.Add(ActionBottleDebtors, null);
		w1.Add(ActionIncomingCallsAnalysisReport, null);
		w1.Add(ActionRoboatsCallsRegistry, null);
		w1.Add(ActionDriversTareMessages, null);

		//Логистика
		w1.Add(ActionRouteListTable, null);
		w1.Add(ActionAtWorks, null);
		w1.Add(ActionRouteListsAtDay, null);
		w1.Add(ActionRouteListsPrint, null);
		w1.Add(ActionRouteListClosingTable, null);
		w1.Add(ActionRouteListTracking, null);
		w1.Add(ActionRouteListMileageCheck, null);

		w1.Add(ActionFinesJournal, null);
		w1.Add(ActionPremiumJournal, null);
		w1.Add(ActionCarProxiesJournal, null);
		w1.Add(ActionRevisionBottlesAndDeposits, null);
		w1.Add(ActionReportDebtorsBottles, null);
		w1.Add(ActionFastDeliveryAvailabilityJournal, null);
		w1.Add(ActionDriversStopLists, null);

		//Бухгалтерия
		w1.Add(ActionTransferBankDocs, null);
		w1.Add(ActionPaymentFromBank, null);
		w1.Add(ActionAccountingTable, null);
		w1.Add(ActionAccountFlow, null);
		w1.Add(ActionRevision, null);
		w1.Add(ActionExportTo1c, null);
		w1.Add(ActionOldExportTo1c, null);
		w1.Add(ActionExportCounterpartiesTo1c, null);
		w1.Add(ActionImportPaymentsByCard, null);
		w1.Add(ActionFinancialDistrictsSetsJournal, null);
		w1.Add(ActionUnallocatedBalancesJournal, null);
		w1.Add(ActionImportPaymentsFromAvangard, null);

		w1.Add(ActionResidue, null);
		w1.Add(ActionEmployeeWorkChart, null);
		w1.Add(ActionRouteListAddressesTransferring, null);
		w1.Add(ActionTransferOperationJournal, null);
		w1.Add(ActionDistricts, null);

		//Касса
		w1.Add(ActionCashDocuments, null);
		w1.Add(ActionAccountableDebt, null);
		w1.Add(ActionUnclosedAdvances, null);
		w1.Add(ActionCashFlow, null);
		w1.Add(ActionSelfdeliveryOrders, null);
		w1.Add(ActionCashTransferDocuments, null);
		w1.Add(ActionFuelTransferDocuments, null);
		w1.Add(ActionOrganizationCashTransferDocuments, null);
		w1.Add(ActionSalariesJournal, null);

		//Suppliers
		w1.Add(ActionNewRequestToSupplier, null);
		w1.Add(ActionJournalOfRequestsToSuppliers, null);
		w1.Add(ActionExportImportNomenclatureCatalog, null);
		w1.Add(ActionWarehousesBalanceSummary, null);

		//ТрО
		w1.Add(ActionCarEventsJournal, null);
		w1.Add(ActionCarProxiesJournal, null);
		w1.Add(ActionRouteListMileageCheck, null);
		w1.Add(ActionWayBillReport, null);
		w1.Add(ActionFinesJournal, null);
		w1.Add(ActionWarehouseDocuments, null);
		w1.Add(ActionWarehouseStock, null);
		w1.Add(ActionCar, null);

		//Отдел продаж
		w1.Add(ActionSalesOrdersJournal, null);
		w1.Add(ActionSalesCounterpartiesJournal, null);
		w1.Add(ActionSalesUndeliveredOrdersJournal, null);
		w1.Add(ActionSalesComplaintsJournal, null);

		UIManager.InsertActionGroup(w1, 0);
		#endregion
		#region Creating events
		//Заказы
		ActionOrdersTable.Activated += ActionOrdersTableActivated;
		ActionAddOrder.Activated += ActionAddOrder_Activated;
		ActionLoadOrders.Activated += ActionLoadOrders_Activated;
		ActionDeliveryPrice.Activated += ActionDeliveryPrice_Activated;
		ActionUndeliveredOrders.Activated += ActionUndeliveredOrdersActivated;
		ActionCashReceiptsJournal.Activated += ActionCashReceiptsJournalActivated;
		ActionOrdersWithReceiptJournal.Activated += ActionOrdersWithReceiptJournalActivated;
		
		ActionServiceClaims.Activated += ActionServiceClaimsActivated;
		ActionWarehouseDocuments.Activated += ActionWarehouseDocumentsActivated;
		ActionReadyForShipment.Activated += ActionReadyForShipmentActivated;
		ActionReadyForReception.Activated += ActionReadyForReceptionActivated;
		ActionWarehouseStock.Activated += ActionWarehouseStock_Activated;
		ActionClientBalance.Activated += ActionClientBalance_Activated;
		ActionWarehouseDocumentsItemsJournal.Activated += ActionWarehouseDocumentsItemsJournal_Activated;

		//Работа с клиентами
		ActionCallTasks.Activated += ActionCallTasks_Activate;
		ActionBottleDebtors.Activated += ActionBottleDebtors_Activate;
		ActionIncomingCallsAnalysisReport.Activated += OnActionIncomingCallsAnalysisReportActivated;
		ActionRoboatsCallsRegistry.Activated += ActionRoboatsCallsRegistryActivated;

		ActionDriversTareMessages.Activated += OnActionDriversTareMessagesActivated;
		//Логистика
		ActionRouteListTable.Activated += ActionRouteListTable_Activated;
		ActionAtWorks.Activated += ActionAtWorks_Activated;
		ActionRouteListsAtDay.Activated += ActionRouteListsAtDay_Activated;
		ActionRouteListsPrint.Activated += ActionRouteListsPrint_Activated;
		ActionRouteListClosingTable.Activated += ActionRouteListClosingTable_Activated;
		ActionRouteListMileageCheck.Activated += ActionRouteListDistanceValidation_Activated;
		ActionRouteListTracking.Activated += ActionRouteListTracking_Activated;
		ActionFastDeliveryAvailabilityJournal.Activated += ActionFastDeliveryAvailabilityJournal_Activated;
		ActionDriversStopLists.Activated += OnActionDriversStopListsActivated;

		ActionFinesJournal.Activated += ActionFinesJournal_Activated;
		ActionPremiumJournal.Activated += ActionPremiumJournal_Activated;
		ActionCarProxiesJournal.Activated += ActionCarProxiesJournal_Activated;
		ActionRevisionBottlesAndDeposits.Activated += ActionRevisionBottlesAndDeposits_Activated;
		ActionReportDebtorsBottles.Activated += ActionReportDebtorsBottles_Activated;

		//Бухгалтерия
		ActionPaymentFromBank.Activated += ActionPaymentFromBank_Activated;
		ActionRevision.Activated += ActionRevision_Activated;
		ActionExportTo1c.Activated += ActionExportTo1c_Activated;
		ActionOldExportTo1c.Activated += ActionOldExportTo1c_Activated;
		ActionExportCounterpartiesTo1c.Activated += ActionExportCounterpartiesTo1c_Activated;
		ActionImportPaymentsByCard.Activated += ActionImportPaymentsByCardActivated;
		ActionFinancialDistrictsSetsJournal.Activated += ActionFinancialDistrictsSetsJournal_Activated;
		ActionUnallocatedBalancesJournal.Activated += OnActionUnallocatedBalancesJournalActivated;
		ActionImportPaymentsFromAvangard.Activated += OnActionImportPaymentsFromAvangardActivated;

		ActionResidue.Activated += ActionResidueActivated;
		ActionEmployeeWorkChart.Activated += ActionEmployeeWorkChart_Activated;
		ActionRouteListAddressesTransferring.Activated += ActionRouteListAddressesTransferring_Activated;
		ActionTransferOperationJournal.Activated += ActionTransferOperationJournal_Activated;
		ActionDistricts.Activated += ActionDistrictsActivated;

		//Касса
		ActionCashDocuments.Activated += ActionCashDocuments_Activated;
		ActionAccountableDebt.Activated += ActionAccountableDebt_Activated;
		ActionUnclosedAdvances.Activated += ActionUnclosedAdvances_Activated;
		ActionCashFlow.Activated += ActionCashFlow_Activated;
		ActionSelfdeliveryOrders.Activated += ActionSelfdeliveryOrders_Activated;
		ActionCashTransferDocuments.Activated += ActionCashTransferDocuments_Activated;
		ActionFuelTransferDocuments.Activated += ActionFuelTransferDocuments_Activated;
		ActionOrganizationCashTransferDocuments.Activated += ActionOrganizationCashTransferDocuments_Activated;
		ActionSalariesJournal.Activated += ActionSalariesJournal_Activated;

		//Suppliers
		ActionNewRequestToSupplier.Activated += ActionNewRequestToSupplier_Activated;
		ActionNewRequestToSupplier.Sensitive =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RequestToSupplier)).CanCreate;
		ActionJournalOfRequestsToSuppliers.Activated += ActionJournalOfRequestsToSuppliers_Activated;
		ActionExportImportNomenclatureCatalog.Activated += ActionExportImportNomenclatureCatalog_Activated;
		ActionWarehousesBalanceSummary.Activated += ActionWarehousesBalanceSummary_Activated;

		//ТрО
		ActionCarEventsJournal.Activated += ActionCarEventsJournalActivated;

		//Отдел продаж
		ActionSalesOrdersJournal.Activated += OnActionSalesOrdersJournalActivated;
		ActionSalesCounterpartiesJournal.Activated += OnActionSalesCounterpartiesJournalActivated;
		ActionSalesUndeliveredOrdersJournal.Activated += OnActionSalesUndeliveredOrdersOrdersJournalActivated;
		ActionSalesComplaintsJournal.Activated += OnActionSalesComplaintsJournalActivated;

		#endregion
	}

	private void OnActionDriversStopListsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DriversStopListsViewModel>(null);
	}

	private void ActionWarehouseDocumentsItemsJournal_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WarehouseDocumentsItemsJournalViewModel>(null);
	}

	private void ActionRoboatsCallsRegistryActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboatsCallsRegistryJournalViewModel>(null);
	}

	private void ActionCashReceiptsJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CashReceiptsJournalViewModel>(null);
	}

	private void ActionOrdersWithReceiptJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrdersWithReceiptsJournalViewModel>(null);
	}

	private void OnActionIncomingCallsAnalysisReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<IncomingCallsAnalysisReportViewModel>(null);
	}

	private void OnActionDriversTareMessagesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DriverTareMessagesJournalViewModel>(null);
	}
	private void ActionSalariesJournal_Activated(object sender, EventArgs e)
	{
		var filter = new SalaryByEmployeeJournalFilterViewModel(new SubdivisionRepository(new ParametersProvider()), EmployeeStatus.IsWorking);

		var page = NavigationManager.OpenViewModel<SalaryByEmployeeJournalViewModel, SalaryByEmployeeJournalFilterViewModel>(null, filter);
		page.ViewModel.SelectionMode = JournalSelectionMode.Single;
	}

	private void ActionWarehousesBalanceSummary_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WarehousesBalanceSummaryViewModel>(null);
	}

	/// <summary>
	/// Новая заявка поставщику
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	void ActionNewRequestToSupplier_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<RequestToSupplierViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());
	}

	void ActionJournalOfRequestsToSuppliers_Activated(object sender, System.EventArgs e)
	{
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		var userRepository = new UserRepository();
		var counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), counterpartyJournalFactory, nomenclatureRepository, userRepository,
				Startup.AppDIContainer.BeginLifetimeScope());

		RequestsToSuppliersFilterViewModel filter = new RequestsToSuppliersFilterViewModel(nomenclatureSelectorFactory);

		NavigationManager.OpenViewModel<RequestsToSuppliersJournalViewModel, RequestsToSuppliersFilterViewModel>(
			  null,
			  filter,
			  OpenPageOptions.IgnoreHash);
	}

	void ActionRouteListsPrint_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<PrintRouteDocumentsDlg>(),
			() => new PrintRouteDocumentsDlg()
		);
	}

	void ActionCallTasks_Activate(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			"CRM",
			() => new TasksView(
								new EmployeeJournalFactory(NavigationManager),
								new DeliveryPointRepository()), null
		);
	}

	void ActionBottleDebtors_Activate(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<DebtorsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	void ActionRouteListAddressesTransferring_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenTdiTab<RouteListAddressesTransferringDlg>(null);
	}

	void ActionEmployeeWorkChart_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<EmployeeWorkChartDlg>(),
			() => new EmployeeWorkChartDlg()
		);
	}

	void ActionLoadOrders_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<LoadFrom1cDlg>(),
			() => new LoadFrom1cDlg()
		);
	}

	void ActionRevisionBottlesAndDeposits_Activated(object sender, System.EventArgs e)
	{
		var scope = Startup.AppDIContainer.BeginLifetimeScope();

		var report = scope.Resolve<RevisionBottlesAndDeposits>();

		var reportViewDlg = new QSReport.ReportViewDlg(report);

		tdiMain.AddTab(reportViewDlg);

		report.Destroyed += (s, args) => scope.Dispose();
	}

	void ActionReportDebtorsBottles_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.ReportDebtorsBottles>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.ReportDebtorsBottles())
		);
	}

	void ActionExportImportNomenclatureCatalog_Activated(object sender, System.EventArgs e)
	{
		INomenclatureRepository nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));

		tdiMain.OpenTab(
			"ExportImportNomenclatureCatalog",
			() => new ExportImportNomenclatureCatalogViewModel(
				nomenclatureRepository,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				NavigationManagerProvider.NavigationManager
			)
		);
	}

	void ActionUnclosedAdvances_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenTdiTab<UnclosedAdvancesView>(null);
	}

	void ActionPaymentFromBank_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PaymentsJournalViewModel>(null);
	}

	private void OnActionImportPaymentsFromAvangardActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ImportPaymentsFromAvangardSbpViewModel>(null);
	}

	private void OnActionUnallocatedBalancesJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UnallocatedBalancesJournalViewModel>(null);
	}

	void ActionFinancialDistrictsSetsJournal_Activated(object sender, EventArgs e)
	{
		var filter = new FinancialDistrictsSetsJournalFilterViewModel { HidenByDefault = true };

		var paymentsJournalViewModel = new FinancialDistrictsSetsJournalViewModel(
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			new EntityDeleteWorker(),
			true,
			true
		);

		tdiMain.AddTab(paymentsJournalViewModel);
	}


	void ActionCashFlow_Activated(object sender, System.EventArgs e)
	{
		var scope = Startup.AppDIContainer.BeginLifetimeScope();

		var report = scope.Resolve<Vodovoz.Reports.CashFlow>();

		var page = NavigationManager.OpenTdiTab<ReportViewDlg, IParametersWidget>(null, report);

		report.ParentTab = page.TdiTab;
	}

	void ActionSelfdeliveryOrders_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<SelfDeliveriesJournalViewModel, Action<OrderJournalFilterViewModel>>(null, filter =>
		{
			filter.AllowStatuses = new[] { OrderStatus.WaitForPayment, OrderStatus.OnLoading, OrderStatus.Accepted, OrderStatus.Closed };
			filter.RestrictOnlySelfDelivery = true;
			filter.RestrictWithoutSelfDelivery = false;
			filter.RestrictHideService = true;
			filter.RestrictOnlyService = false;
			filter.RestrictLessThreeHours = false;
			filter.SortDeliveryDate = false;
		}, OpenPageOptions.IgnoreHash);
	}

	void ActionCashTransferDocuments_Activated(object sender, System.EventArgs e) =>
		NavigationManager.OpenViewModel<TransferDocumentsJournalViewModel>(null);

	void ActionFuelTransferDocuments_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FuelDocumentsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	void ActionOrganizationCashTransferDocuments_Activated(object sender, EventArgs e)
	{
		var entityExtendedPermissionValidator = new EntityExtendedPermissionValidator(
			PermissionExtensionSingletonStore.GetInstance(), new EmployeeRepository());

		var employeeFilter = new EmployeeFilterViewModel
		{
			Status = EmployeeStatus.IsWorking,
		};

		var employeeJournalFactory = new EmployeeJournalFactory(NavigationManager, employeeFilter);

		tdiMain.OpenTab(() => new OrganizationCashTransferDocumentJournalViewModel(
			new OrganizationCashTransferDocumentFilterViewModel(employeeJournalFactory)
			{
				HidenByDefault = true
			},
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			entityExtendedPermissionValidator,
			VodovozGtkServicesConfig.EmployeeService)
		);
	}

	void ActionFinesJournal_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<FinesJournalViewModel>(null);
	}

	void ActionPremiumJournal_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<PremiumJournalViewModel, Action<SubdivisionFilterViewModel>, Action<PremiumJournalFilterViewModel>>(
			null,
			subdivisionFilter =>
			{
				subdivisionFilter.SubdivisionType = SubdivisionType.Default;
			},
			premiumFilter =>
			{
				premiumFilter.HidenByDefault = true;
			});
	}

	void ActionCarProxiesJournal_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ProxyDocumentsView>(),
			() => new ProxyDocumentsView()
		);
	}

	void ActionRevision_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(RevisionReportViewModel));
	}

	void ActionExportTo1c_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ExportTo1cDialog>(),
			() => new ExportTo1cDialog(UnitOfWorkFactory.GetDefaultFactory)
		);
	}

	void ActionOldExportTo1c_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<Old1612ExportTo1cDialog>(),
			() => new Old1612ExportTo1cDialog(UnitOfWorkFactory.GetDefaultFactory)
		);
	}

	void ActionExportCounterpartiesTo1c_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ExportCounterpartiesTo1cDlg>(),
			() => new ExportCounterpartiesTo1cDlg()
		);
	}

	void ActionImportPaymentsByCardActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ImportPaymentsFromTinkoffDlg>(),
			() => new ImportPaymentsFromTinkoffDlg()
		);
	}

	void ActionAccountableDebt_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenTdiTab<AccountableDebts>(null);
	}

	void ActionRouteListClosingTable_Activated(object sender, EventArgs e)
	{
		var filter = new RouteListJournalFilterViewModel();
		filter.StartDate = DateTime.Today.AddMonths(-2);
		filter.EndDate = DateTime.Today.AddDays(1);
		
		var page = NavigationManager.OpenViewModel<RouteListWorkingJournalViewModel, RouteListJournalFilterViewModel>(null, filter);
		page.ViewModel.NavigationManager = NavigationManager;
	}

	void ActionRouteListTracking_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<CarsMonitoringViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	void ActionRouteListDistanceValidation_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenTdiTab<RouteListMileageCheckJournalViewModel>(null);
	}

	void ActionCashDocuments_Activated(object sender, System.EventArgs e) =>
		NavigationManager.OpenViewModel<DocumentsJournalViewModel>(null);

	void ActionReadyForShipmentActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReadyForShipmentView>(),
			() => new ReadyForShipmentView()
		);
	}

	void ActionReadyForReceptionActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReadyForReceptionView>(),
			() => new ReadyForReceptionView()
		);
	}

	void ActionClientBalance_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			PermissionControlledRepresentationJournal.GenerateHashName<ClientEquipmentBalanceVM>(),
			() =>
			{
				var journal = new PermissionControlledRepresentationJournal(new ClientEquipmentBalanceVM());
				journal.CustomTabName("Оборудование у клиентов");
				return journal;
			}
		);
	}

	void ActionAddOrder_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			DialogHelper.GenerateDialogHashName<Order>(0),
			() => new OrderDlg() { IsForRetail = false }
		);
	}

	void ActionWarehouseStock_Activated(object sender, System.EventArgs e)
	{
		var defaultWarehouse = CurrentUserSettings.Settings.DefaultWarehouse;
		Action<NomenclatureStockFilterViewModel> filterParams = null;
		
		if(_accessOnlyToWarehouseAndComplaints && defaultWarehouse != null)
		{
			filterParams = f =>
			{
				f.RestrictWarehouse = defaultWarehouse;
				f.ShowArchive = true;
			};
		}
		else
		{
			filterParams = f => f.ShowArchive = true;
		}
		
		NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(
			null, filterParams);
	}

	void ActionWarehouseDocumentsActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<WarehouseDocumentsView>(),
			() => new WarehouseDocumentsView()
		);
	}

	void ActionServiceClaimsActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ServiceClaimsView>(),
			() => new ServiceClaimsView()
		);
	}

	void ActionOrdersTableActivated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<OrderJournalViewModel, Action<OrderJournalFilterViewModel>>(null, filter =>
		{
			filter.IsForRetail = false;
		}, OpenPageOptions.IgnoreHash);
	}

	void ActionUndeliveredOrdersActivated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(null, config =>
		{
			config.HidenByDefault = true;
			config.RestrictUndeliveryStatus = UndeliveryStatus.InProcess;
			config.RestrictNotIsProblematicCases = true;
		}, OpenPageOptions.IgnoreHash);
	}

	void ActionResidueActivated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<ResidueJournalViewModel>(null);
	}

	void ActionTransferOperationJournal_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			PermissionControlledRepresentationJournal.GenerateHashName<TransferOperationsVM>(),
			() => new PermissionControlledRepresentationJournal(new TransferOperationsVM()).CustomTabName("Переносы между точками доставки")
		);
	}

	void ActionDeliveryPrice_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<DeliveryPriceDlg>(),
			() => new DeliveryPriceDlg()
		);
	}

	void ActionDistrictsActivated(object sender, System.EventArgs e)
	{
		var filter = new DistrictsSetJournalFilterViewModel { HidenByDefault = true };
		NavigationManager.OpenViewModel<DistrictsSetJournalViewModel, DistrictsSetJournalFilterViewModel>(null, filter);
	}

	void ActionCarEventsJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarEventJournalViewModel, Action<CarEventFilterViewModel>>(null, filter => filter.HidenByDefault = true);
	}

	void ActionFastDeliveryAvailabilityJournal_Activated(object sender, EventArgs e)
	{
		IEmployeeJournalFactory employeeJournalFactory = new EmployeeJournalFactory(NavigationManager);
		IDistrictJournalFactory districtJournalFactory = new DistrictJournalFactory();
		ICounterpartyJournalFactory counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());
		IFileDialogService fileDialogService = new FileDialogService();
		IFastDeliveryAvailabilityHistoryParameterProvider fastDeliveryAvailabilityHistoryParameterProvider =
			new FastDeliveryAvailabilityHistoryParameterProvider(new ParametersProvider());
		INomenclatureParametersProvider nomenclatureParametersProvider = new NomenclatureParametersProvider(new ParametersProvider());

		var filter = new FastDeliveryAvailabilityFilterViewModel(counterpartyJournalFactory, employeeJournalFactory, districtJournalFactory)
		{
			VerificationDateFrom = DateTime.Now.Date,
			VerificationDateTo = DateTime.Now.Date.Add(new TimeSpan(23, 59, 59))
		};

		tdiMain.OpenTab(() => new FastDeliveryAvailabilityHistoryJournalViewModel(
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			fileDialogService,
			fastDeliveryAvailabilityHistoryParameterProvider,
			nomenclatureParametersProvider)
		);
	}

	void OnActionSalesOrdersJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrderJournalViewModel, Action<OrderJournalFilterViewModel>>(null, filter =>
		{
			filter.IsForSalesDepartment = true;
		});
	}

	void OnActionSalesCounterpartiesJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CounterpartyJournalViewModel, Action<CounterpartyJournalFilterViewModel>>(null, filter =>
		{
			filter.IsForSalesDepartment = true;
		});
	}

	void OnActionSalesUndeliveredOrdersOrdersJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(null, filter =>
		{
			filter.RestrictUndeliveryStatus = UndeliveryStatus.InProcess;
			filter.RestrictNotIsProblematicCases = true;
			filter.IsForSalesDepartment = true;
		}, OpenPageOptions.IgnoreHash);
	}

	void OnActionSalesComplaintsJournalActivated(object sender, EventArgs e)
	{
		Action<ComplaintFilterViewModel> action = (filterConfig) => filterConfig.IsForSalesDepartment = true;

		var filter = _autofacScope.BeginLifetimeScope().Resolve<ComplaintFilterViewModel>(new TypedParameter(typeof(Action<ComplaintFilterViewModel>), action));

		NavigationManager.OpenViewModel<ComplaintsJournalViewModel, ComplaintFilterViewModel>(
			   null,
			   filter,
			   OpenPageOptions.IgnoreHash);
	}
}
