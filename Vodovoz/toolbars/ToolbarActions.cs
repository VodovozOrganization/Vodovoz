using Dialogs.Employees;
using Gtk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI.FileDialog;
using QS.Project.Services.FileDialog;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.Core.Journal;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Chats;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalFilters.Cash;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.JournalViewModels.Suppliers;
using Vodovoz.Old1612ExportTo1c;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Representations;
using Vodovoz.ServiceDialogs;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels;
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
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using VodovozInfrastructure.Endpoints;
using Action = Gtk.Action;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;

public partial class MainWindow : Window
{
	//Заказы
	Action ActionOrdersTable;
	Action ActionAddOrder;
	Action ActionLoadOrders;
	Action ActionDeliveryPrice;
	Action ActionUndeliveredOrders;

	Action ActionServiceClaims;
	Action ActionWarehouseDocuments;
	Action ActionWarehouseStock;
	Action ActionClientBalance;

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

	public void BuildToolbarActions()
	{
		#region Creating actions

		//Заказы
		ActionOrdersTable = new Action("ActionOrdersTable", "Журнал заказов", null, "table");
		ActionAddOrder = new Action("ActionAddOrder", "Новый заказ", null, "table");
		ActionLoadOrders = new Action("ActionLoadOrders", "Загрузить из 1С", null, "table");
		ActionDeliveryPrice = new Action("ActionDeliveryPrice", "Стоимость доставки", null, null);
		ActionUndeliveredOrders = new Action("ActionUndeliveredOrders", "Журнал недовозов", null, null);

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
		ActionWarehousesBalanceSummary = new Action(nameof(ActionWarehousesBalanceSummary), "Остатки по складам", null, "table");

		//ТрО
		ActionCarEventsJournal = new Action("ActionCarEventsJournal", "Журнал событий ТС", null, "table");

		#endregion
		#region Inserting actions to the toolbar
		ActionGroup w1 = new ActionGroup("ToolbarActions");

		//Заказы
		w1.Add(ActionOrdersTable, null);
		w1.Add(ActionAddOrder, null);
		w1.Add(ActionLoadOrders, null);
		w1.Add(ActionDeliveryPrice, null);
		w1.Add(ActionUndeliveredOrders, null);

		//
		w1.Add(ActionServiceClaims, null);
		w1.Add(ActionWarehouseDocuments, null);
		w1.Add(ActionReadyForShipment, null);
		w1.Add(ActionReadyForReception, null);
		w1.Add(ActionWarehouseStock, null);
		w1.Add(ActionClientBalance, null);

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

		UIManager.InsertActionGroup(w1, 0);
		#endregion
		#region Creating events
		//Заказы
		ActionOrdersTable.Activated += ActionOrdersTableActivated;
		ActionAddOrder.Activated += ActionAddOrder_Activated;
		ActionLoadOrders.Activated += ActionLoadOrders_Activated;
		ActionDeliveryPrice.Activated += ActionDeliveryPrice_Activated;
		ActionUndeliveredOrders.Activated += ActionUndeliveredOrdersActivated;

		ActionServiceClaims.Activated += ActionServiceClaimsActivated;
		ActionWarehouseDocuments.Activated += ActionWarehouseDocumentsActivated;
		ActionReadyForShipment.Activated += ActionReadyForShipmentActivated;
		ActionReadyForReception.Activated += ActionReadyForReceptionActivated;
		ActionWarehouseStock.Activated += ActionWarehouseStock_Activated;
		ActionClientBalance.Activated += ActionClientBalance_Activated;

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

		ActionFinesJournal.Activated += ActionFinesJournal_Activated;
		ActionPremiumJournal.Activated += ActionPremiumJournal_Activated;
		ActionCarProxiesJournal.Activated += ActionCarProxiesJournal_Activated;
		ActionRevisionBottlesAndDeposits.Activated += ActionRevisionBottlesAndDeposits_Activated;
		ActionReportDebtorsBottles.Activated += ActionReportDebtorsBottles_Activated;

		//Бухгалтерия
		ActionTransferBankDocs.Activated += ActionTransferBankDocs_Activated;
		ActionPaymentFromBank.Activated += ActionPaymentFromBank_Activated;
		ActionAccountingTable.Activated += ActionAccountingTable_Activated;
		ActionAccountFlow.Activated += ActionAccountFlow_Activated;
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

		#endregion
	}

	private void ActionRoboatsCallsRegistryActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboatsCallsRegistryJournalViewModel>(null);
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

	void ActionNewRequestToSupplier_Activated(object sender, System.EventArgs e) {
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		var userRepository = new UserRepository();
		var counterpartyJournalFactory = new CounterpartyJournalFactory();

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), counterpartyJournalFactory, nomenclatureRepository, userRepository);

		tdiMain.OpenTab(
			DialogHelper.GenerateDialogHashName<RequestToSupplier>(0),
			() => new RequestToSupplierViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				VodovozGtkServicesConfig.EmployeeService,
				new SupplierPriceItemsRepository(),
				counterpartyJournalFactory,
				new NomenclatureJournalFactory(),
				nomenclatureRepository,
				userRepository
			)
		);
	}

	void ActionJournalOfRequestsToSuppliers_Activated(object sender, System.EventArgs e) {
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		var userRepository = new UserRepository();
		var counterpartyJournalFactory = new CounterpartyJournalFactory();

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), counterpartyJournalFactory, nomenclatureRepository, userRepository);

		RequestsToSuppliersFilterViewModel filter = new RequestsToSuppliersFilterViewModel(nomenclatureSelectorFactory);

		var requestsJournal = new RequestsToSuppliersJournalViewModel(
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			new SupplierPriceItemsRepository(),
			counterpartyJournalFactory,
			new NomenclatureJournalFactory(),
			nomenclatureRepository,
			userRepository
		);
		tdiMain.AddTab(requestsJournal);
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
								new EmployeeJournalFactory(),
								new DeliveryPointRepository()), null
		);
	}

	void ActionBottleDebtors_Activate(object sender, System.EventArgs e)
	{
		DebtorsJournalFilterViewModel filter = new DebtorsJournalFilterViewModel();
		IEmailParametersProvider emailParametersProvider = new EmailParametersProvider(new ParametersProvider());
		IAttachmentsViewModelFactory attachmentsViewModelFactory = new AttachmentsViewModelFactory();
		IEmailRepository emailRepository = new EmailRepository();
		IFileDialogService fileDialogService = new FileDialogService();
		var debtorsJournal = new DebtorsJournalViewModel(
			filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, new EmployeeRepository(), new GtkTabsOpener(),
			new DebtorsParameters(new ParametersProvider()), emailParametersProvider, attachmentsViewModelFactory, emailRepository, fileDialogService);

		tdiMain.AddTab(debtorsJournal);
	}

	void ActionRouteListAddressesTransferring_Activated(object sender, System.EventArgs e)
	{
		var parametersProvider = new ParametersProvider();
		var employeeNomenclatureMovementRepository = new EmployeeNomenclatureMovementRepository();
		var terminalNomenclatureProvider = new BaseParametersProvider(parametersProvider);
		var routeListRepository = new RouteListRepository(new StockRepository(), new BaseParametersProvider(parametersProvider));
		var routeListItemRepository = new RouteListItemRepository();
		var employeeService = new EmployeeService();

		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListAddressesTransferringDlg>(),
			() => new RouteListAddressesTransferringDlg(
				employeeNomenclatureMovementRepository,
				terminalNomenclatureProvider,
				routeListRepository,
				routeListItemRepository,
				employeeService,
				ServicesConfig.CommonServices,
				new CategoryRepository(parametersProvider)
			)
		);
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
		var reportViewDlg = new QSReport.ReportViewDlg(new Vodovoz.Reports.RevisionBottlesAndDeposits(
				new OrderRepository(), new CounterpartyJournalFactory(), new DeliveryPointJournalFactory()));

		tdiMain.AddTab(reportViewDlg);
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

	void ActionAtWorks_Activated(object sender, EventArgs e)
	{
		var employeeJournalFactory = new EmployeeJournalFactory();

		var cs = new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider> { new MemoryConfigurationProvider(new MemoryConfigurationSource()) }), "");

		cs["BaseUri"] = "https://driverapi.vod.qsolution.ru:7090/api/";

		var apiHelper = new ApiClientProvider.ApiClientProvider(cs);
		var driverApiRegisterEndpoint = new DriverApiUserRegisterEndpoint(apiHelper);
		var parametersProvider = new ParametersProvider();

		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AtWorksDlg>(),
			() => new AtWorksDlg(
				new BaseParametersProvider(parametersProvider),
				employeeJournalFactory,
				driverApiRegisterEndpoint,
				new GeographicGroupParametersProvider(parametersProvider))
		);
	}

	void ActionRouteListsAtDay_Activated(object sender, System.EventArgs e)
	{
		var parametersProvider = new ParametersProvider();
		var baseParametersProvider = new BaseParametersProvider(parametersProvider);

		tdiMain.OpenTab(
			"AutoRouting",
			() => new RouteListsOnDayViewModel(
				ServicesConfig.CommonServices,
				new DeliveryScheduleParametersProvider(parametersProvider),
				new GtkTabsOpener(),
				new RouteListRepository(new StockRepository(), baseParametersProvider),
				new SubdivisionRepository(parametersProvider),
				new OrderRepository(),
				new AtWorkRepository(),
				new CarRepository(),
				NavigationManagerProvider.NavigationManager,
				new UserRepository(),
				baseParametersProvider,
				new EmployeeJournalFactory(),
				new GeographicGroupRepository(),
				new ScheduleRestrictionRepository(),
				new CarModelJournalFactory(),
				new GeographicGroupParametersProvider(parametersProvider)
			)
		);
	}

	void ActionAccountingTable_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AccountingView>(),
			() => new AccountingView()
		);
	}

	void ActionUnclosedAdvances_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<UnclosedAdvancesView>(),
			() => new UnclosedAdvancesView()
		);
	}

	void ActionTransferBankDocs_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<LoadBankTransferDocumentDlg>(),
			() => new LoadBankTransferDocumentDlg()
		);
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
		var parametersProvider = new ParametersProvider();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.CashFlow>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.CashFlow(
				new SubdivisionRepository(parametersProvider), ServicesConfig.CommonServices, new CategoryRepository(parametersProvider)))
		);
	}

	void ActionSelfdeliveryOrders_Activated(object sender, System.EventArgs e)
	{
		var counterpartyJournalFactory = new CounterpartyJournalFactory();
		var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
		var parametersProvider = new ParametersProvider();

		var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory);

		filter.SetAndRefilterAtOnce(
			x => x.AllowStatuses = new [] { OrderStatus.WaitForPayment, OrderStatus.OnLoading, OrderStatus.Accepted, OrderStatus.Closed },
			x => x.RestrictOnlySelfDelivery = true,
			x => x.RestrictWithoutSelfDelivery = false,
			x => x.RestrictHideService = true,
			x => x.RestrictOnlyService = false,
			x => x.RestrictLessThreeHours = false,
			x => x.SortDeliveryDate = false
		);
		filter.HidenByDefault = true;
		var selfDeliveriesJournal = new SelfDeliveriesJournalViewModel(
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			new CallTaskWorker(CallTaskSingletonFactory.GetInstance(),
				new CallTaskRepository(),
				new OrderRepository(),
				new EmployeeRepository(),
				new BaseParametersProvider(parametersProvider),
				ServicesConfig.CommonServices.UserService,
				ErrorReporter.Instance),
            new OrderPaymentSettings(parametersProvider),
			new OrderParametersProvider(parametersProvider),
			new DeliveryRulesParametersProvider(parametersProvider),
			VodovozGtkServicesConfig.EmployeeService
		);

		tdiMain.AddTab(selfDeliveriesJournal);
	}

	void ActionCashTransferDocuments_Activated(object sender, System.EventArgs e)
	{
		var cashRepository = new CashRepository();

		tdiMain.OpenTab(
			RepresentationJournalDialog.GenerateHashName<CashTransferDocumentVM>(),
			() => {
				var vm = new CashTransferDocumentVM(
					UnitOfWorkFactory.GetDefaultFactory,
                    new CashTransferDocumentsFilter(),
					cashRepository,
					new ParametersProvider());

				return new MultipleEntityJournal("Журнал перемещения д/с", vm, vm);
			}
		);
	}

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

		var employeeJournalFactory = new EmployeeJournalFactory(employeeFilter);

		tdiMain.OpenTab(() => new OrganizationCashTransferDocumentJournalViewModel(
			new OrganizationCashTransferDocumentFilterViewModel(employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory())
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

		tdiMain.OpenTab(
			PermissionControlledRepresentationJournal.GenerateHashName<FinesVM>(),
			() => {
				FinesVM vm = new FinesVM();
				vm.Filter.SetAndRefilterAtOnce(f => f.SetFilterDates(System.DateTime.Today.AddMonths(-2), System.DateTime.Today));
				Buttons buttons = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_fines")
														  ? Buttons.All
														  : (Buttons.Add | Buttons.Edit);
				return new PermissionControlledRepresentationJournal(vm, buttons, true).CustomTabName("Журнал штрафов");
			}
		);
	}

	void ActionPremiumJournal_Activated(object sender, System.EventArgs e)
	{
		IEmployeeJournalFactory employeeJournalFactory = new EmployeeJournalFactory();
		IPremiumTemplateJournalFactory premiumTemplateJournalFactory = new PremiumTemplateJournalFactory();

		var subdivisionAutocompleteSelectorFactory =
			new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), () =>
			{
				return new SubdivisionsJournalViewModel(
					new SubdivisionFilterViewModel() { SubdivisionType = SubdivisionType.Default },
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
					new SalesPlanJournalFactory(),
					new NomenclatureJournalFactory()
				);
			});

		tdiMain.OpenTab(() => new PremiumJournalViewModel(
			new PremiumJournalFilterViewModel(subdivisionAutocompleteSelectorFactory) { HidenByDefault = true },
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			employeeJournalFactory,
			premiumTemplateJournalFactory
			)
		);
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
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Revision>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Revision())
		);
	}

	void ActionAccountFlow_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.AccountFlow>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.AccountFlow(new CategoryRepository(new ParametersProvider())))
		);
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
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AccountableDebts>(),
			() => new AccountableDebts()
		);
	}

	void ActionRouteListTable_Activated(object sender, System.EventArgs e)
	{
		var filter = new RouteListJournalFilterViewModel();
		filter.StartDate = DateTime.Today.AddMonths(-2);
		filter.EndDate = DateTime.Today;

		NavigationManager.OpenViewModel<RouteListJournalViewModel, RouteListJournalFilterViewModel>(null, filter);
	}

	void ActionRouteListClosingTable_Activated(object sender, EventArgs e)
	{
		var page = NavigationManager.OpenViewModel<RouteListWorkingJournalViewModel>(null);
		page.ViewModel.NavigationManager = NavigationManager;
	}

	void ActionRouteListTracking_Activated(object sender, System.EventArgs e)
	{
		var employeeRepository = new EmployeeRepository();
		var chatRepository = new ChatRepository();
		var trackRepository = new TrackRepository();
		var stockRepository = new StockRepository();
		var terminalNomenclatureProvider = new BaseParametersProvider(new ParametersProvider());
		var routeListRepository = new RouteListRepository(stockRepository, terminalNomenclatureProvider);
		var scheduleRestrictionRepository = new ScheduleRestrictionRepository();
		var deliveryRulesParametersProvider = new DeliveryRulesParametersProvider(new ParametersProvider());

		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListTrackDlg>(),
			() => new RouteListTrackDlg(employeeRepository, chatRepository, trackRepository, routeListRepository, scheduleRestrictionRepository,
				deliveryRulesParametersProvider, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices)
		);
	}

	void ActionRouteListDistanceValidation_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenTdiTab<RouteListMileageCheckView>(null);
	}

	void ActionCashDocuments_Activated(object sender, System.EventArgs e)
	{
		var cashRepository = new CashRepository();

		tdiMain.OpenTab(
			RepresentationJournalDialog.GenerateHashName<CashMultipleDocumentVM>(),
			() => {
				var vm = new CashMultipleDocumentVM(new CashDocumentsFilter(), cashRepository);
				return new MultipleEntityJournal("Журнал кассовых документов", vm, vm);
			}
		);
	}

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
			() => {
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
		bool userHasOnlyAccessToWarehouseAndComplaints;

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(uow).IsAdmin;
		}

		var defaultWarehouse = CurrentUserSettings.Settings.DefaultWarehouse;
		NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(new WarehouseSelectorFactory())
		{
			ShowArchive = true
		};

		if(userHasOnlyAccessToWarehouseAndComplaints && defaultWarehouse != null)
		{
			filter.RestrictWarehouse = defaultWarehouse;
		}

		NomenclatureStockBalanceJournalViewModel vm = new NomenclatureStockBalanceJournalViewModel(
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices
		) {SelectionMode = JournalSelectionMode.None};

		tdiMain.OpenTab(() => vm);
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
		var counterpartyJournalFactory = new CounterpartyJournalFactory();
		var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
		var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory)
		{
			IsForRetail = false
		};

		NavigationManager.OpenViewModel<OrderJournalViewModel, OrderJournalFilterViewModel>(null, filter, OpenPageOptions.IgnoreHash);
	}

	void ActionUndeliveredOrdersActivated(object sender, System.EventArgs e)
	{
		ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

		var undeliveredOrdersFilter = new UndeliveredOrdersFilterViewModel(ServicesConfig.CommonServices, new OrderSelectorFactory(),
			new EmployeeJournalFactory(), new CounterpartyJournalFactory(), new DeliveryPointJournalFactory(), subdivisionJournalFactory)
		{
			HidenByDefault = true,
			RestrictUndeliveryStatus = UndeliveryStatus.InProcess,
			RestrictNotIsProblematicCases = true
		};

		NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, UndeliveredOrdersFilterViewModel>(null, undeliveredOrdersFilter, OpenPageOptions.IgnoreHash);
	}

	void ActionResidueActivated(object sender, System.EventArgs e)
	{
		IMoneyRepository moneyRepository = new MoneyRepository();
		IDepositRepository depositRepository = new DepositRepository();
		IBottlesRepository bottlesRepository = new BottlesRepository();
		ResidueFilterViewModel filter = new ResidueFilterViewModel();
		var employeeJournalFactory = new EmployeeJournalFactory();

		var residueJournalViewModel = new ResidueJournalViewModel(
			filter,
			VodovozGtkServicesConfig.EmployeeService,
			VodovozGtkServicesConfig.RepresentationEntityPicker,
			moneyRepository,
			depositRepository,
			bottlesRepository,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory()
		);
		tdiMain.AddTab(residueJournalViewModel);
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
		ICarJournalFactory carJournalFactory = new CarJournalFactory(NavigationManager);
		IEmployeeJournalFactory employeeFactory = new EmployeeJournalFactory();
		ICarEventTypeJournalFactory carEventTypeJournalFactory = new CarEventTypeJournalFactory();

		var carEventFilter = new CarEventFilterViewModel(
			carJournalFactory,
			carEventTypeJournalFactory,
			new EmployeeJournalFactory())
		{ HidenByDefault = true };

		tdiMain.OpenTab(() => new CarEventJournalViewModel(
			carEventFilter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			carJournalFactory,
			carEventTypeJournalFactory,
			VodovozGtkServicesConfig.EmployeeService,
			employeeFactory,
			new UndeliveredOrdersJournalOpener(),
			new EmployeeSettings(new ParametersProvider()))
		);
	}

	void ActionFastDeliveryAvailabilityJournal_Activated(object sender, EventArgs e)
	{
		IEmployeeJournalFactory employeeJournalFactory = new EmployeeJournalFactory();
		IDistrictJournalFactory districtJournalFactory = new DistrictJournalFactory();
		ICounterpartyJournalFactory counterpartyJournalFactory = new CounterpartyJournalFactory();
		IFileDialogService fileDialogService = new FileDialogService();
		IFastDeliveryAvailabilityHistoryParameterProvider fastDeliveryAvailabilityHistoryParameterProvider =
			new FastDeliveryAvailabilityHistoryParameterProvider(new ParametersProvider());
		INomenclatureParametersProvider nomenclatureParametersProvider = new NomenclatureParametersProvider(new ParametersProvider());

		var filter = new FastDeliveryAvailabilityFilterViewModel(counterpartyJournalFactory, employeeJournalFactory, districtJournalFactory)
		{
			HidenByDefault = true,
			VerificationDateFrom = DateTime.Now.Date,
			VerificationDateTo = DateTime.Now.Date.Add(new TimeSpan(23,59,59))
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
}
