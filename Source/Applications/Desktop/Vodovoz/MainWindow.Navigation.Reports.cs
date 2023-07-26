using Autofac;
using QS.Dialog;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Report.ViewModels;
using QSOrmProject;
using System;
using Vodovoz;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Parameters;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.ViewModels.ViewModels.Warehouses;

public partial class MainWindow
{
	#region Заказы

	/// <summary>
	/// Отчет по комментариям кассы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCashierCommentsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.CashierCommentsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.CashierCommentsReport()));
	}

	/// <summary>
	/// Отчет по районам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersByDistrict(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByDistrictReport()));
	}

	/// <summary>
	/// Отчет по последнему заказу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionLastOrderReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<LastOrderByDeliveryPointReport>(),
			() => new QSReport.ReportViewDlg(new LastOrderByDeliveryPointReport()));
	}

	/// <summary>
	/// Отчет по некорректным ценам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderIncorrectPricesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderIncorrectPrices>(),
			() => new QSReport.ReportViewDlg(new OrderIncorrectPrices()));
	}

	/// <summary>
	/// Отчёт по заказам меньше 100 р.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersWithMinPriceLessThanActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersWithMinPriceLessThan>(),
			() => new QSReport.ReportViewDlg(new OrdersWithMinPriceLessThan()));
	}

	/// <summary>
	/// Отчёт по самовывозу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSelfDeliveryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SelfDeliveryReport>(),
			() => new QSReport.ReportViewDlg(new SelfDeliveryReport()));
	}

	/// <summary>
	/// Отчет по ценам пригорода
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSuburbWaterPriceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SuburbWaterPriceReport>(),
			() => new QSReport.ReportViewDlg(new SuburbWaterPriceReport()));
	}

	/// <summary>
	/// Теги контрагентов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCounterpartyTagsActivated(object sender, EventArgs e)
	{
		var refWin = new OrmReference(typeof(Tag));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Отчет по недовозам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNotDeliveredOrdersActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NotDeliveredOrdersReport>(),
			() => new QSReport.ReportViewDlg(new NotDeliveredOrdersReport()));
	}

	/// <summary>
	/// Реестр заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderRegistryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderRegistryReport>(),
			() => new QSReport.ReportViewDlg(new OrderRegistryReport()));
	}

	/// <summary>
	/// Оплата по картам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCardPaymentsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CardPaymentsOrdersReport>(),
			() => new QSReport.ReportViewDlg(new CardPaymentsOrdersReport(UnitOfWorkFactory.GetDefaultFactory)));
	}

	/// <summary>
	/// Оплаты OnLine заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnOnLineActionActivated(object sender, EventArgs e)
	{
		var paymentsRepository = new PaymentsRepository();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromTinkoffReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromTinkoffReport(paymentsRepository)));
	}

	/// <summary>
	/// Первичные клиенты
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFirstClientsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FirstClientsReport>(),
			() => new QSReport.ReportViewDlg(
				  new FirstClientsReport(
						new DistrictJournalFactory(),
						new DiscountReasonRepository())));
	}

	/// <summary>
	/// Отчет по скидкам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSalesByDicountReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SalesByDiscountReport>(),
			() => new QSReport.ReportViewDlg(new SalesByDiscountReport()));
	}

	/// <summary>
	/// Отчет по потенциальным халявщикам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction66Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PotentialFreePromosetsReport>(),
			() => new QSReport.ReportViewDlg(new PotentialFreePromosetsReport()));
	}

	/// <summary>
	/// Отчет по оплатам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPaymentsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientReport>(),
			() => new QSReport.ReportViewDlg(
				new PaymentsFromBankClientReport(new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()), new UserRepository(), ServicesConfig.CommonServices)));
	}

	/// <summary>
	/// Отчет по заказам ИМ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction71Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EShopSalesReport>(),
			() => new QSReport.ReportViewDlg(new EShopSalesReport()));
	}

	/// <summary>
	/// Аналитика заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderAnalyticsReportActivated(object sender, EventArgs e)
	{
		var uowFactory = autofacScope.Resolve<IUnitOfWorkFactory>();
		var interactiveService = autofacScope.Resolve<IInteractiveService>();

		NavigationManager.OpenViewModel<OrderAnalyticsReportViewModel, INavigationManager, IUnitOfWorkFactory, IInteractiveService>(
			null, NavigationManager, uowFactory, interactiveService);
	}

	/// <summary>
	/// Отчет по мотивации КЦ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNomenclaturePlanReportActivated(object sender, EventArgs e)
	{
		IProductGroupJournalFactory productGroupJournalFactory = new ProductGroupJournalFactory();
		IParametersProvider parametersProvider = new ParametersProvider();
		INomenclaturePlanParametersProvider nomenclaturePlanParametersProvider = new NomenclaturePlanParametersProvider(parametersProvider);
		IFileDialogService fileDialogService = new FileDialogService();

		NomenclaturePlanReportViewModel viewModel = new NomenclaturePlanReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, ServicesConfig.CommonServices, productGroupJournalFactory, nomenclaturePlanParametersProvider,
			fileDialogService);

		tdiMain.AddTab(viewModel);
	}

	/// <summary>
	/// Оплаты из Авангарда
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPaymentsFromAvangardReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromAvangardReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromAvangardReport()));
	}

	#endregion Заказы

	#region Продажи

	/// <summary>
	/// Отчет по продажам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSalesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.SalesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.SalesReport(new EmployeeRepository(), ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Отчет по дате создания заказа
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderCreationDateReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderCreationDateReport>(),
			() => new QSReport.ReportViewDlg(new OrderCreationDateReport()));
	}

	/// <summary>
	/// Отчёт о выполнении плана
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPlanImplementationReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PlanImplementationReport>(),
			() => new QSReport.ReportViewDlg(new PlanImplementationReport()));
	}

	/// <summary>
	/// Отчет по выставленным счетам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSetBillsReportActivated(object sender, EventArgs e)
	{
		var subdivisionJournalFactory = new SubdivisionJournalFactory();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SetBillsReport>(),
			() => new QSReport.ReportViewDlg(new SetBillsReport(
				UnitOfWorkFactory.GetDefaultFactory,
				subdivisionJournalFactory)));
	}

	/// <summary>
	/// Отчет по продажам с рентабельностью
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ActionProfitabilitySalesReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProfitabilitySalesReportViewModel));
	}

	/// <summary>
	/// Отчет по оборачиваемости с динамикой
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionTurnoverWithDynamicsReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<TurnoverWithDynamicsReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Аналитика продаж КБ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnSalesBySubdivisionsAnalitycsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SalesBySubdivisionsAnalitycsReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Продажи

	#region Склад

	/// <summary>
	/// Остатки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionActionWarehousesBalanceSummaryReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WarehousesBalanceSummaryViewModel>(null);
	}

	/// <summary>
	/// Складские движения
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionStockMovementsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.StockMovements>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.StockMovements()));
	}

	/// <summary>
	/// ТМЦ на остатках
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEquipmentBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EquipmentBalance>(),
			() => new QSReport.ReportViewDlg(new EquipmentBalance()));
	}

	/// <summary>
	/// Отчёт по браку
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDefectiveItemsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DefectiveItemsReport>(),
			() => new QSReport.ReportViewDlg(new DefectiveItemsReport()));
	}

	/// <summary>
	/// Недопогруженные МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNotFullyLoadedRouteListsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NotFullyLoadedRouteListsReport>(),
			() => new QSReport.ReportViewDlg(new NotFullyLoadedRouteListsReport()));
	}

	/// <summary>
	/// Товары для отгрузки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnForShipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NomenclatureForShipment>(),
			() => new QSReport.ReportViewDlg(new NomenclatureForShipment()));
	}

	/// <summary>
	/// Развернутые движения ТМЦ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionStockMovementsAdvancedReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<StockMovementsAdvancedReport>(),
			() => new QSReport.ReportViewDlg(new StockMovementsAdvancedReport()));
	}

	/// <summary>
	/// Заявка на производство
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionProductionRequestReportActivated(object sender, EventArgs e)
	{
		var employeeRepository = new EmployeeRepository();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProductionRequestReport>(),
			() => new QSReport.ReportViewDlg(new ProductionRequestReport(employeeRepository)));
	}

	/// <summary>
	/// Движение по инвентарному номеру
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnInventoryInstanceMovementReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryInstanceMovementReportViewModel>(null);
	}

	#endregion Склад

	#region Отчеты ОСК/ОКК

	/// <summary>
	/// Отчет по движению бутылей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionBottlesMovementSummaryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<BottlesMovementSummaryReport>(),
			() => new QSReport.ReportViewDlg(new BottlesMovementSummaryReport()));
	}

	/// <summary>
	/// Отчет по движению бутылей (по МЛ)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionBottlesMovementReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<BottlesMovementReport>(),
			() => new QSReport.ReportViewDlg(new BottlesMovementReport()));
	}

	/// <summary>
	/// Отчет о несданных бутылях
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionShortfallBattlesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ShortfallBattlesReport>(),
			() => new QSReport.ReportViewDlg(new ShortfallBattlesReport()));
	}

	/// <summary>
	/// Отчёт "Куньголово"
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnReportKungolovoActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReportForBigClient>(),
			() => new QSReport.ReportViewDlg(new ReportForBigClient()));
	}

	/// <summary>
	/// Отчет по дате создания заказа
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersByCreationDate(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByCreationDateReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByCreationDateReport()));
	}

	/// <summary>
	/// Отчет по тарифным зонам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionTariffZoneDebtsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<TariffZoneDebts>(),
			() => new QSReport.ReportViewDlg(new TariffZoneDebts()));
	}

	/// <summary>
	/// Реестр МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderedByIdRoutesListRegisterActivated(object sender, EventArgs e) => OpenRoutesListRegisterReport();

	/// <summary>
	/// Клиенты по типам объектов и видам деятельности
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyActivityKindActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ClientsByDeliveryPointCategoryAndActivityKindsReport>(),
			() => new QSReport.ReportViewDlg(new ClientsByDeliveryPointCategoryAndActivityKindsReport()));
	}

	/// <summary>
	/// Отчет по пересданной таре водителями
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionExtraBottlesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ExtraBottleReport>(),
			() => new QSReport.ReportViewDlg(new ExtraBottleReport()));
	}

	/// <summary>
	/// Отчет по первичным/вторичным заказам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFirstSecondReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FirstSecondClientReport>(),
			() => new QSReport.ReportViewDlg(new FirstSecondClientReport(new DiscountReasonRepository())));
	}

	/// <summary>
	/// Рентабельность акции "Бутыль"
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionProfitabilityBottlesByStockActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProfitabilityBottlesByStockReport>(),
			() => new QSReport.ReportViewDlg(new ProfitabilityBottlesByStockReport()));
	}

	/// <summary>
	/// Отчет по нулевому долгу клиента
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionZeroDebtClientReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ZeroDebtClientReport>(),
			() => new QSReport.ReportViewDlg(new ZeroDebtClientReport()));
	}

	/// <summary>
	/// Отчет по забору тары
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionReturnedTareReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReturnedTareReport>(),
			() => new QSReport.ReportViewDlg(new ReturnedTareReport(ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Отчет о событиях рассылки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionBulkEmailEventsReportActivated(object sender, EventArgs e)
	{
		ICounterpartyJournalFactory counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());
		IBulkEmailEventReasonJournalFactory bulkEmailEventReasonJournalFactory = new BulkEmailEventReasonJournalFactory();
		IFileDialogService fileDialogService = new FileDialogService();

		BulkEmailEventReportViewModel viewModel = new BulkEmailEventReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, fileDialogService, bulkEmailEventReasonJournalFactory, counterpartyJournalFactory);

		tdiMain.AddTab(viewModel);
	}

	#endregion Отчеты ОСК/ОКК
}
