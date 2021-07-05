using System;
using System.Collections.Generic;
using System.Data;
using Dialogs.Employees;
using Gtk;
using InstantSmsService;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
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
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels.Suppliers;
using Vodovoz.Representations;
using Vodovoz.ServiceDialogs;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.EntityRepositories.Store;
using QS.Project.Journal;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;
using QS.ViewModels;
using Vodovoz.Additions;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewWidgets;
using VodovozInfrastructure.Interfaces;
using Action = Gtk.Action;
using Vodovoz.Old1612ExportTo1c;
using Vodovoz.JournalFilters.Cash;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.PermissionExtensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

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

	//CRM
	Action ActionCallTasks;
	Action ActionBottleDebtors;

	//Логистика
	Action ActionRouteListTable;
	Action ActionAtWorks;
	Action ActionRouteListsAtDay;
	Action ActionRouteListsPrint;
	Action ActionRouteListClosingTable;
	Action ActionRouteListKeeping;
	Action ActionRouteListMileageCheck;
	Action ActionRouteListTracking;

	Action ActionReadyForShipment;
	Action ActionReadyForReception;
	Action ActionCashDocuments;
	Action ActionAccountableDebt;
	Action ActionUnclosedAdvances;
	Action ActionCashFlow;
	Action ActionSelfdeliveryOrders;
	Action ActionFinesJournal;
	Action ActionPremiumJournal;
	Action ActionCarProxiesJournal;
	Action ActionRevision;
	Action ActionRevisionBottlesAndDeposits;
	Action ActionReportDebtorsBottles;
	Action ActionExportImportNomenclatureCatalog;
	Action ActionTransferBankDocs;
	Action ActionPaymentFromBank;
	Action ActionFinancialDistrictsSetsJournal;
	Action ActionAccountingTable;
	Action ActionAccountFlow;
	Action ActionExportTo1c;
	Action ActionOldExportTo1c;
	Action ActionExportCounterpartiesTo1c;
	Action ActionImportPaymentsByCard;
	Action ActionResidue;
	Action ActionEmployeeWorkChart;
	Action ActionRouteListAddressesTransferring;
	Action ActionTransferOperationJournal;
	Action ActionDistricts;
	Action ActionCashTransferDocuments;
	Action ActionFuelTransferDocuments;
	Action ActionOrganizationCashTransferDocuments;

	//Suppliers
	Action ActionNewRequestToSupplier;
	Action ActionJournalOfRequestsToSuppliers;

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

		//CRM
		ActionCallTasks = new Action("ActionCallTasks", "Журнал задач", null, "table");
		ActionBottleDebtors = new Action("ActionBottleDebtors", "Журнал задолженности", null, "table");

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
		ActionRouteListKeeping = new Action("ActionRouteListKeeping", "Ведение маршрутных листов", null, "table");
		ActionRouteListMileageCheck = new Action("ActionRouteListMileageCheck", "Контроль за километражем", null, "table");
		ActionRouteListAddressesTransferring = new Action("ActionRouteListAddressesTransferring", "Перенос адресов", null, "table");

		//Касса
		ActionCashDocuments = new Action("ActionCashDocuments", "Кассовые документы", null, "table");
		ActionAccountableDebt = new Action("ActionAccountableDebt", "Долги сотрудников", null, "table");
		ActionUnclosedAdvances = new Action("ActionUnclosedAdvances", "Незакрытые авансы", null, "table");
		ActionCashFlow = new Action("ActionCashFlow", "Доходы и расходы", null, "table");
		ActionSelfdeliveryOrders = new Action("ActionSelfdeliveryOrders", "Журнал самовывозов", null, "table");
		ActionCashTransferDocuments = new Action("ActionCashTransferDocuments", "Журнал перемещения д/с", null, "table");
		ActionFuelTransferDocuments = new Action("ActionFuelTransferDocuments", "Журнал учета топлива", null, "table");
		ActionOrganizationCashTransferDocuments = new Action("ActionOrganizationCashTransferDocuments", "Журнал перемещения д/с для юр.лиц", null, "table");

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

		//CRM
		w1.Add(ActionCallTasks, null);
		w1.Add(ActionBottleDebtors, null);

		//Логистика
		w1.Add(ActionRouteListTable, null);
		w1.Add(ActionAtWorks, null);
		w1.Add(ActionRouteListsAtDay, null);
		w1.Add(ActionRouteListsPrint, null);
		w1.Add(ActionRouteListClosingTable, null);
		w1.Add(ActionRouteListKeeping, null);
		w1.Add(ActionRouteListTracking, null);
		w1.Add(ActionRouteListMileageCheck, null);

		w1.Add(ActionCashDocuments, null);
		w1.Add(ActionAccountableDebt, null);
		w1.Add(ActionUnclosedAdvances, null);
		w1.Add(ActionCashFlow, null);
		w1.Add(ActionSelfdeliveryOrders, null);
		w1.Add(ActionCashTransferDocuments, null);
		w1.Add(ActionFuelTransferDocuments, null);
		w1.Add(ActionOrganizationCashTransferDocuments, null);
		w1.Add(ActionFinesJournal, null);
		w1.Add(ActionPremiumJournal, null);
		w1.Add(ActionCarProxiesJournal, null);
		w1.Add(ActionRevision, null);
		w1.Add(ActionRevisionBottlesAndDeposits, null);
		w1.Add(ActionReportDebtorsBottles, null);
		w1.Add(ActionTransferBankDocs, null);
		w1.Add(ActionPaymentFromBank, null);
		w1.Add(ActionFinancialDistrictsSetsJournal, null);
		w1.Add(ActionAccountingTable, null);
		w1.Add(ActionAccountFlow, null);
		w1.Add(ActionExportTo1c, null);
		w1.Add(ActionOldExportTo1c, null);
		w1.Add(ActionExportCounterpartiesTo1c, null);
		w1.Add(ActionImportPaymentsByCard, null);
		w1.Add(ActionResidue, null);
		w1.Add(ActionEmployeeWorkChart, null);
		w1.Add(ActionRouteListAddressesTransferring, null);
		w1.Add(ActionTransferOperationJournal, null);
		w1.Add(ActionDistricts, null);

		//Suppliers
		w1.Add(ActionNewRequestToSupplier, null);
		w1.Add(ActionJournalOfRequestsToSuppliers, null);
		w1.Add(ActionExportImportNomenclatureCatalog, null);

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

		//CRM
		ActionCallTasks.Activated += ActionCallTasks_Activate;
		ActionBottleDebtors.Activated += ActionBottleDebtors_Activate;

		//Логистика
		ActionRouteListTable.Activated += ActionRouteListTable_Activated;
		ActionAtWorks.Activated += ActionAtWorks_Activated;
		ActionRouteListsAtDay.Activated += ActionRouteListsAtDay_Activated;
		ActionRouteListsPrint.Activated += ActionRouteListsPrint_Activated;
		ActionRouteListClosingTable.Activated += ActionRouteListClosingTable_Activated;
		ActionRouteListKeeping.Activated += ActionRouteListKeeping_Activated;
		ActionRouteListMileageCheck.Activated += ActionRouteListDistanceValidation_Activated;
		ActionRouteListTracking.Activated += ActionRouteListTracking_Activated;

		ActionCashDocuments.Activated += ActionCashDocuments_Activated;
		ActionAccountableDebt.Activated += ActionAccountableDebt_Activated;
		ActionUnclosedAdvances.Activated += ActionUnclosedAdvances_Activated;
		ActionCashFlow.Activated += ActionCashFlow_Activated;
		ActionSelfdeliveryOrders.Activated += ActionSelfdeliveryOrders_Activated;
		ActionCashTransferDocuments.Activated += ActionCashTransferDocuments_Activated;
		ActionFuelTransferDocuments.Activated += ActionFuelTransferDocuments_Activated;
		ActionOrganizationCashTransferDocuments.Activated += ActionOrganizationCashTransferDocuments_Activated;
		ActionFinesJournal.Activated += ActionFinesJournal_Activated;
		ActionPremiumJournal.Activated += ActionPremiumJournal_Activated;
		ActionCarProxiesJournal.Activated += ActionCarProxiesJournal_Activated;
		ActionRevision.Activated += ActionRevision_Activated;
		ActionRevisionBottlesAndDeposits.Activated += ActionRevisionBottlesAndDeposits_Activated;
		ActionReportDebtorsBottles.Activated += ActionReportDebtorsBottles_Activated;
		ActionTransferBankDocs.Activated += ActionTransferBankDocs_Activated;
		ActionPaymentFromBank.Activated += ActionPaymentFromBank_Activated;
		ActionFinancialDistrictsSetsJournal.Activated += ActionFinancialDistrictsSetsJournal_Activated;
		ActionAccountingTable.Activated += ActionAccountingTable_Activated;
		ActionAccountFlow.Activated += ActionAccountFlow_Activated;
		ActionExportTo1c.Activated += ActionExportTo1c_Activated;
		ActionOldExportTo1c.Activated += ActionOldExportTo1c_Activated;
		ActionExportCounterpartiesTo1c.Activated += ActionExportCounterpartiesTo1c_Activated;
		ActionImportPaymentsByCard.Activated += ActionImportPaymentsByCardActivated;
		ActionResidue.Activated += ActionResidueActivated;
		ActionEmployeeWorkChart.Activated += ActionEmployeeWorkChart_Activated;
		ActionRouteListAddressesTransferring.Activated += ActionRouteListAddressesTransferring_Activated;
		ActionTransferOperationJournal.Activated += ActionTransferOperationJournal_Activated;
		ActionDistricts.Activated += ActionDistrictsActivated;

		//Suppliers
		ActionNewRequestToSupplier.Activated += ActionNewRequestToSupplier_Activated;
		ActionJournalOfRequestsToSuppliers.Activated += ActionJournalOfRequestsToSuppliers_Activated;
		ActionExportImportNomenclatureCatalog.Activated += ActionExportImportNomenclatureCatalog_Activated;

		//ТрО
		ActionCarEventsJournal.Activated += ActionCarEventsJournalActivated;
		
		#endregion
	}

	void ActionNewRequestToSupplier_Activated(object sender, System.EventArgs e) {
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
		
		IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
				CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);
		
		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService),
				counterpartySelectorFactory, nomenclatureRepository, UserSingletonRepository.GetInstance());
		
		tdiMain.OpenTab(
			DialogHelper.GenerateDialogHashName<RequestToSupplier>(0),
			() => new RequestToSupplierViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				VodovozGtkServicesConfig.EmployeeService,
				new SupplierPriceItemsRepository(),
				counterpartySelectorFactory,
				nomenclatureSelectorFactory,
				nomenclatureRepository,
				UserSingletonRepository.GetInstance()
			)
		);
	}

	void ActionJournalOfRequestsToSuppliers_Activated(object sender, System.EventArgs e) {
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
		
		IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
				CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService),
				counterpartySelectorFactory, nomenclatureRepository, UserSingletonRepository.GetInstance());
		
		RequestsToSuppliersFilterViewModel filter = new RequestsToSuppliersFilterViewModel(nomenclatureSelectorFactory);
		
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		var requestsJournal = new RequestsToSuppliersJournalViewModel(
			journalActions,
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			new SupplierPriceItemsRepository(),
			counterpartySelectorFactory,
			nomenclatureSelectorFactory,
			nomenclatureRepository,
			UserSingletonRepository.GetInstance()
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
			() => new TasksView(EmployeeSingletonRepository.GetInstance(), 
								new BottlesRepository(),
								new CallTaskRepository(),
								new PhoneRepository()), null
		);

		/*
		tdiMain.OpenTab(
			"CRM",
			() => new BusinessTasksJournalViewModel(new CallTaskFilterViewModel(),
													new BusinessTasksJournalFooterViewModel(),
													UnitOfWorkFactory.GetDefaultFactory,
													ServicesConfig.CommonServices,
													EmployeeSingletonRepository.GetInstance(),
													new BottlesRepository(),
													new CallTaskRepository(),
													new PhoneRepository()) { SelectionMode = JournalSelectionMode.Multiple}, null
		);
	*/
	}

	void ActionBottleDebtors_Activate(object sender, System.EventArgs e)
	{
		DebtorsJournalFilterViewModel filter = new DebtorsJournalFilterViewModel();
		var journalActions = new DebtorsJournalActionsViewModel();

		var debtorsJournal = new DebtorsJournalViewModel(
			journalActions,
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			EmployeeSingletonRepository.GetInstance());

		tdiMain.AddTab(debtorsJournal);
	}

	void ActionRouteListAddressesTransferring_Activated(object sender, System.EventArgs e) {
		var employeeNomenclatureMovementRepository = new EmployeeNomenclatureMovementRepository();
		var terminalNomenclatureProvider = new BaseParametersProvider();
		
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListAddressesTransferringDlg>(),
			() => new RouteListAddressesTransferringDlg(
				employeeNomenclatureMovementRepository,
				terminalNomenclatureProvider,
				VodovozGtkServicesConfig.EmployeeService,
				ServicesConfig.CommonServices
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
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.RevisionBottlesAndDeposits>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.RevisionBottlesAndDeposits())
		);
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
		INomenclatureRepository nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());

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

	void ActionAtWorks_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AtWorksDlg>(),
			() => new AtWorksDlg(new BaseParametersProvider())
		);
	}

	void ActionRouteListsAtDay_Activated(object sender, System.EventArgs e)
	{
		if(new BaseParametersProvider().UseOldAutorouting())
			tdiMain.OpenTab(
				TdiTabBase.GenerateHashName<RoutesAtDayDlg>(),
				() => new RoutesAtDayDlg()
			);
		else
			tdiMain.OpenTab(
				"AutoRouting",
				() => new RouteListsOnDayViewModel(
					ServicesConfig.CommonServices,
					new DeliveryScheduleParametersProvider(SingletonParametersProvider.Instance),
					new GtkTabsOpener(),
					new RouteListRepository(),
					new SubdivisionRepository(),
					OrderSingletonRepository.GetInstance(),
					new AtWorkRepository(),
					new CarRepository(),
					NavigationManagerProvider.NavigationManager,
					UserSingletonRepository.GetInstance(),
					new BaseParametersProvider()
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

	void ActionPaymentFromBank_Activated(object sender, System.EventArgs e)
	{
		var filter = new PaymentsJournalFilterViewModel();
		var journalActions = new PaymentsJournalActionsViewModel(ServicesConfig.InteractiveService, UnitOfWorkFactory.GetDefaultFactory);

		var paymentsJournalViewModel = new PaymentsJournalViewModel(
			journalActions,
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			NavigationManagerProvider.NavigationManager,
			OrderSingletonRepository.GetInstance(),
			new OrganizationParametersProvider(SingletonParametersProvider.Instance),
			new BaseParametersProvider()
		);

		tdiMain.AddTab(paymentsJournalViewModel);
	}

	void ActionFinancialDistrictsSetsJournal_Activated(object sender, EventArgs e)
	{
		var filter = new FinancialDistrictsSetsJournalFilterViewModel
		{
			HidenByDefault = true
		};
		var journalActions = 
			new FinancialDistrictsSetJournalActionsViewModel(
				ServicesConfig.CommonServices, VodovozGtkServicesConfig.EmployeeService, UnitOfWorkFactory.GetDefaultFactory);

		var paymentsJournalViewModel = new FinancialDistrictsSetsJournalViewModel(
			journalActions,
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
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.CashFlow>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.CashFlow(new SubdivisionRepository(), ServicesConfig.CommonServices))
		);
	}

	void ActionSelfdeliveryOrders_Activated(object sender, System.EventArgs e)
	{
		OrderJournalFilterViewModel filter = new OrderJournalFilterViewModel();
		filter.SetAndRefilterAtOnce(
			x => x.AllowStatuses = new OrderStatus[] { OrderStatus.WaitForPayment, OrderStatus.OnLoading, OrderStatus.Accepted, OrderStatus.Closed },
			x => x.RestrictOnlySelfDelivery = true,
			x => x.RestrictWithoutSelfDelivery = false,
			x => x.RestrictHideService = true,
			x => x.RestrictOnlyService = false,
			x => x.RestrictLessThreeHours = false
		);
		filter.HidenByDefault = true;
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		var selfDeliveriesJournal = new SelfDeliveriesJournalViewModel(
			journalActions,
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			new CallTaskWorker(
				CallTaskSingletonFactory.GetInstance(),
				new CallTaskRepository(),
				OrderSingletonRepository.GetInstance(),
				EmployeeSingletonRepository.GetInstance(),
				new BaseParametersProvider(),
				ServicesConfig.CommonServices.UserService,
				SingletonErrorReporter.Instance
			),
            new OrderPaymentSettings());
		
		tdiMain.AddTab(selfDeliveriesJournal);
	}

	void ActionCashTransferDocuments_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			RepresentationJournalDialog.GenerateHashName<CashTransferDocumentVM>(),
			() => {
				var vm = new CashTransferDocumentVM(UnitOfWorkFactory.GetDefaultFactory,
                    new CashTransferDocumentsFilter());
				return new MultipleEntityJournal("Журнал перемещения д/с", vm, vm);
			}
		);
	}

	void ActionFuelTransferDocuments_Activated(object sender, System.EventArgs e)
	{
		ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();
		IFuelRepository fuelRepository = new FuelRepository();
		ICounterpartyJournalFactory counterpartyJournalFactory = new CounterpartyJournalFactory();
		INomenclatureSelectorFactory nomenclatureSelectorFactory = new NomenclatureSelectorFactory();
		var employeesJournalActions =
			new EmployeesJournalActionsViewModel(ServicesConfig.InteractiveService, UnitOfWorkFactory.GetDefaultFactory);
		var employeesJournalFilter = new EmployeeFilterViewModel();
		IEmployeeJournalFactory employeeJournalFactory = new EmployeeJournalFactory(employeesJournalActions, employeesJournalFilter);
		ICarJournalFactory carJournalFactory = new CarJournalFactory();
		
		IFileChooserProvider fileChooserProvider = new Vodovoz.FileChooser("Категория Расхода.csv");
		var  expenseCategoryJournalFilterViewModel = new ExpenseCategoryJournalFilterViewModel(); 
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		var fuelDocumentsJournalViewModel = new FuelDocumentsJournalViewModel(
			journalActions,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			subdivisionRepository,
			fuelRepository,
			counterpartyJournalFactory,
			nomenclatureSelectorFactory,
			employeeJournalFactory,
			carJournalFactory,
			new GtkReportViewOpener(),
			fileChooserProvider,
			expenseCategoryJournalFilterViewModel
		);
		tdiMain.AddTab(fuelDocumentsJournalViewModel);
	}

	void ActionOrganizationCashTransferDocuments_Activated(object sender, System.EventArgs e)
	{
		var employeesJournalActions = 
			new EmployeesJournalActionsViewModel(ServicesConfig.InteractiveService, UnitOfWorkFactory.GetDefaultFactory);
		var organizationCashTransferDocumentJournalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		var entityExtendedPermissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(),
			EmployeeSingletonRepository.GetInstance());

        var employeeSelectorFactory = new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
            () =>
            {
                var employeeFilter = new EmployeeFilterViewModel
                {
                    Status = EmployeeStatus.IsWorking,
                };
                return new EmployeesJournalViewModel(
	                employeesJournalActions,
                    employeeFilter,
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices);
            });

        var orgCashTransferDocFilter = new OrganizationCashTransferDocumentFilterViewModel(employeeSelectorFactory)
        {
	        HidenByDefault = true
        };

        tdiMain.OpenTab(() => new OrganizationCashTransferDocumentJournalViewModel(
	        organizationCashTransferDocumentJournalActions,
	        orgCashTransferDocFilter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			entityExtendedPermissionValidator)
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
				return new PermissionControlledRepresentationJournal(vm, buttons).CustomTabName("Журнал штрафов");
			}
		);
	}

	void ActionPremiumJournal_Activated(object sender, System.EventArgs e)
	{
		var employeesJournalActions =
			new EmployeesJournalActionsViewModel(ServicesConfig.InteractiveService, UnitOfWorkFactory.GetDefaultFactory);
		var employeesJournalFilter = new EmployeeFilterViewModel();
		IEmployeeJournalFactory employeeJournalFactory = new EmployeeJournalFactory(employeesJournalActions, employeesJournalFilter);
		var premiumJournalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
		IPremiumTemplateJournalFactory premiumTemplateJournalFactory = new PremiumTemplateJournalFactory(premiumJournalActions);
		var subdivisionsFilter = new SubdivisionFilterViewModel
		{
			SubdivisionType = SubdivisionType.Default
		};
		var subdivisionsJournalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		var subdivisionAutocompleteSelectorFactory =
			new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), 
				() => new SubdivisionsJournalViewModel(
					subdivisionsJournalActions,
					subdivisionsFilter,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory()
			));

		var premiumFilter = new PremiumJournalFilterViewModel(subdivisionAutocompleteSelectorFactory)
		{
			HidenByDefault = true
		};
		
		tdiMain.OpenTab(() => new PremiumJournalViewModel(
			premiumJournalActions,
			premiumFilter,
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
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.AccountFlow())
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
		tdiMain.OpenTab(
			PermissionControlledRepresentationJournal.GenerateHashName<RouteListsVM>(),
			() => {
				var vm = new RouteListsVM();
				vm.Filter.SetAndRefilterAtOnce(x => x.SetFilterDates(System.DateTime.Today.AddMonths(-2), System.DateTime.Today));
				Buttons buttons = Buttons.Add | Buttons.Edit;
				return new PermissionControlledRepresentationJournal(vm, buttons);
			}
		);
	}

	void ActionRouteListClosingTable_Activated(object sender, System.EventArgs e)
	{
		var routeListFilter = new RouteListJournalFilterViewModel();
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
		
        tdiMain.OpenTab(
            () => new RouteListWorkingJournalViewModel(
	            journalActions,
	            routeListFilter,
	            UnitOfWorkFactory.GetDefaultFactory,
	            ServicesConfig.CommonServices,
	            new RouteListRepository(),
	            new FuelRepository(),
	            new CallTaskRepository(),
	            new BaseParametersProvider(),
	            new SubdivisionRepository()
            )
        );
    }

	void ActionRouteListTracking_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListTrackDlg>(),
			() => new RouteListTrackDlg()
		);
	}

	void ActionRouteListKeeping_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListKeepingView>(),
			() => new RouteListKeepingView()
		);
	}

	void ActionRouteListDistanceValidation_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListMileageCheckView>(),
			() => new RouteListMileageCheckView()
		);
	}

	void ActionCashDocuments_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			RepresentationJournalDialog.GenerateHashName<CashMultipleDocumentVM>(),
			() => {
				var vm = new CashMultipleDocumentVM(new CashDocumentsFilter());
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
		NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(new WarehouseRepository())
		{
			ShowArchive = true
		};
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		NomenclatureStockBalanceJournalViewModel vm = new NomenclatureStockBalanceJournalViewModel(
			journalActions,
			filter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices
		)
		{
			SelectionMode = JournalSelectionMode.None
		};

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

	void ActionOrdersTableActivated(object sender, System.EventArgs e) {
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
		
		IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
				CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);
		
		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService),
				counterpartySelectorFactory, nomenclatureRepository, UserSingletonRepository.GetInstance());
		
		OrderJournalFilterViewModel filter = new OrderJournalFilterViewModel
		{
			IsForRetail = false
		};
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
		
		var ordersJournal = new OrderJournalViewModel(
			journalActions,
			filter, 
			UnitOfWorkFactory.GetDefaultFactory, 
			ServicesConfig.CommonServices,
			VodovozGtkServicesConfig.EmployeeService,
			nomenclatureSelectorFactory,
			counterpartySelectorFactory,
			nomenclatureRepository,
			UserSingletonRepository.GetInstance());
		
		tdiMain.AddTab(ordersJournal);
	}

	void ActionUndeliveredOrdersActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<UndeliveriesView>(),
			() => {
				var view = new UndeliveriesView {
					ButtonMode = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_undeliveries") ? ReferenceButtonMode.CanAll : ReferenceButtonMode.CanAdd
				};
				return view;
			}
		);
	}

	void ActionResidueActivated(object sender, System.EventArgs e)
	{
		IMoneyRepository moneyRepository = new MoneyRepository();
		IDepositRepository depositRepository = new DepositRepository();
		IBottlesRepository bottlesRepository = new BottlesRepository();
		ResidueFilterViewModel filter = new ResidueFilterViewModel();
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

		var residueJournalViewModel = new ResidueJournalViewModel(
			journalActions,
			filter,
			VodovozGtkServicesConfig.EmployeeService,
			VodovozGtkServicesConfig.RepresentationEntityPicker,
			moneyRepository,
			depositRepository,
			bottlesRepository,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices)
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
		var filter = new DistrictsSetJournalFilterViewModel
		{
			HidenByDefault = true
		};
		var journalActions =
			new DistrictsSetJournalActionsViewModel(ServicesConfig.CommonServices, VodovozGtkServicesConfig.EmployeeService,
				UnitOfWorkFactory.GetDefaultFactory);

		tdiMain.OpenTab(
			() => new DistrictsSetJournalViewModel(
				journalActions, filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices,
			EmployeeSingletonRepository.GetInstance(), new EntityDeleteWorker(), true, true)
		);
	}

	void ActionCarEventsJournalActivated(object sender, System.EventArgs e)
	{
		ICarJournalFactory carJournalFactory = new CarJournalFactory();
		ICarEventTypeJournalFactory carEventTypeJournalFactory = new CarEventTypeJournalFactory();

		var carEventFilter = new CarEventFilterViewModel(carJournalFactory, carEventTypeJournalFactory) { HidenByDefault = true };
		var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
		
		tdiMain.OpenTab(() => new CarEventJournalViewModel(
			journalActions,
			carEventFilter,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			carJournalFactory,
			carEventTypeJournalFactory)
		);
	}
}
