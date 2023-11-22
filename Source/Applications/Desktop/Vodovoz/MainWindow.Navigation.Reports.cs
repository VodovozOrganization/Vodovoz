using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report.ViewModels;
using QSOrmProject;
using System;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bookkeeping;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Employees;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Production;
using Vodovoz.ReportsParameters.Retail;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.Reports;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using Vodovoz.ViewModels.ReportsParameters.Payments;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
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
	protected void OnActionCounterpartyTagsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<TagJournalViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(PaymentsFromBankClientReportViewModel));
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
		NavigationManager.OpenViewModel<OrderAnalyticsReportViewModel>(null);
	}

	/// <summary>
	/// Отчет по мотивации КЦ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNomenclaturePlanReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclaturePlanReportViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(SalesReportViewModel));
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
			() => new QSReport.ReportViewDlg(new OrderCreationDateReport(NavigationManager)));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(SetBillsReportViewModel));
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
			() => new QSReport.ReportViewDlg(new DefectiveItemsReport(NavigationManager)));
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
			() => new QSReport.ReportViewDlg(new ShortfallBattlesReport(NavigationManager)));
	}

	/// <summary>
	/// Отчёт "Куньголово"
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnReportKungolovoActivated(object sender, EventArgs e)
	{
		var scope = Startup.AppDIContainer.BeginLifetimeScope();

		var report = scope.Resolve<ReportForBigClient>();

		report.Destroyed += (s, args) => scope?.Dispose();

		var tab = tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReportForBigClient>(),
			() => new QSReport.ReportViewDlg(report));
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
			() => new QSReport.ReportViewDlg(new FirstSecondClientReport(NavigationManager, new DiscountReasonRepository())));
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
		NavigationManager.OpenViewModel<BulkEmailEventReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Отчеты ОСК/ОКК

	#region Логистика

	/// <summary>
	/// Заказы по районам и интервалам доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersByDistrictsAndDeliverySchedulesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictsAndDeliverySchedulesReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByDistrictsAndDeliverySchedulesReport()));
	}

	/// <summary>
	/// Отчет по выдаче топлива по МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelConsumptionReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FuelConsumptionReport>(),
			() => new QSReport.ReportViewDlg(new FuelConsumptionReport())
		);
	}

	/// <summary>
	/// Отчет по времени приема заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersCreationTimeReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersCreationTimeReport>(),
			() => new QSReport.ReportViewDlg(new OrdersCreationTimeReport()));
	}

	/// <summary>
	/// Путевой лист
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionWayBillReportActivated(object sender, EventArgs e)
	{
		var scope = Startup.AppDIContainer.BeginLifetimeScope();

		var viewModel = scope.Resolve<WayBillReportGroupPrint>();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<WayBillReport>(),
			() => new QSReport.ReportViewDlg(viewModel));

		viewModel.Destroyed += (_, _2) => scope?.Dispose();
	}

	/// <summary>
	/// Отчет по незакрытым МЛ за период
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNonClosedRLByPeriodReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NonClosedRLByPeriodReport>(),
			() => new QSReport.ReportViewDlg(new NonClosedRLByPeriodReport()));
	}

	/// <summary>
	/// График выхода на линию за смену
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionScheduleOnLinePerShiftReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FuelConsumptionReport>(),
			() => new QSReport.ReportViewDlg(new ScheduleOnLinePerShiftReport()));
	}

	/// <summary>
	/// Статистика по дням недели
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderStatisticByWeekReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderStatisticByWeekReport>(),
			() => new QSReport.ReportViewDlg(new OrderStatisticByWeekReport()));
	}

	/// <summary>
	/// Аналитика эксплуатации ТС
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCarsExploitationReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarsExploitationReportViewModel>(null);
	}

	/// <summary>
	/// Основная информация по ЗП
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnLogisticsGeneralSalaryInfoActivated(object sender, EventArgs e)
	{
		var filter = new EmployeeFilterViewModel
		{
			Category = EmployeeCategory.driver
		};

		var employeeJournalFactory = new EmployeeJournalFactory(NavigationManager, filter);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<GeneralSalaryInfoReport>(),
			() => new QSReport.ReportViewDlg(new GeneralSalaryInfoReport(employeeJournalFactory, ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Выгрузка по водителям
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriversInfoExportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DriversInfoExportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Отчет по переплатам за адрес
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionAddressesOverpaymentsReportActivated(object sender, EventArgs e)
	{
		var driverFilter = new EmployeeFilterViewModel { RestrictCategory = EmployeeCategory.driver };
		var employeeJournalFactory = new EmployeeJournalFactory(NavigationManager, driverFilter);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<AddressesOverpaymentsReport>(),
			() => new QSReport.ReportViewDlg(new AddressesOverpaymentsReport(
				employeeJournalFactory,
				ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Аналитика объёмов доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveryAnalyticsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DeliveryAnalyticsViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Аналитика по недовозам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionAnalyticsForUndeliveryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<AnalyticsForUndeliveryReport>(),
			() => new QSReport.ReportViewDlg(new AnalyticsForUndeliveryReport()));
	}

	/// <summary>
	/// Отчёт по продажам с доставкой за час
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnFastDeliverySalesReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FastDeliverySalesReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Отчёт по дозагрузке МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnFastDeliveryAdditionalLoadingReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FastDeliveryAdditionalLoadingReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Доступность услуги "Доставка за час"
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFastDeliveryPercentCoverageReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FastDeliveryPercentCoverageReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Логистика

	#region Сотрудники

	/// <summary>
	/// Штрафы сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEmployeeFinesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EmployeesFines>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EmployeesFines()));
	}

	/// <summary>
	/// Премии сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction48Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesPremiums>(),
			() => new QSReport.ReportViewDlg(new EmployeesPremiums()));
	}

	/// <summary>
	/// Отчет по сотрудникам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEmployeesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesReport>(),
			() => new QSReport.ReportViewDlg(new EmployeesReport(ServicesConfig.InteractiveService)));
	}

	#endregion Сотрудники

	#region Водители

	/// <summary>
	/// Отчет по опозданиям
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveriesLateActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.DeliveriesLateReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.DeliveriesLateReport()));
	}

	/// <summary>
	/// Отчет по незакрытым МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRouteListsOnClosingActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<RouteListsOnClosingReport>(),
			() => new QSReport.ReportViewDlg(new RouteListsOnClosingReport()));
	}

	/// <summary>
	/// Реестр маршрутных листов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRoutesListRegisterActivated(object sender, EventArgs e) => OpenDriverRoutesListRegisterReport();

	/// <summary>
	/// Время погрузки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOnLoadTimeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OnLoadTimeAtDayReport>(),
			() => new QSReport.ReportViewDlg(new OnLoadTimeAtDayReport()));
	}

	/// <summary>
	/// Время доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveryTimeReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(QSReport.ReportViewDlg.GenerateHashName<DeliveryTimeReport>(),
			() => new QSReport.ReportViewDlg(
				new DeliveryTimeReport(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Загрузка наших автомобилей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCompanyTrucksActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CompanyTrucksReport>(),
			() => new QSReport.ReportViewDlg(new CompanyTrucksReport()));
	}

	/// <summary>
	/// Отчёт по отгрузке автомобилей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionShipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ShipmentReport>(),
			() => new QSReport.ReportViewDlg(new ShipmentReport()));
	}

	/// <summary>
	/// Отчёт по километражу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMileageReportActivated(object sender, EventArgs e)
	{
		var scope = Startup.AppDIContainer.BeginLifetimeScope();

		var report = scope.Resolve<MileageReport>();

		var tab = tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MileageReport>(),
			() => new QSReport.ReportViewDlg(report));

		report.Destroyed += (_, _2) => scope?.Dispose();
	}

	/// <summary>
	/// Отчёт по водительскому телефону
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriveingCallsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DrivingCallReport>(),
			() => new QSReport.ReportViewDlg(new DrivingCallReport()));
	}

	/// <summary>
	/// Отчет по распределению водителей на районы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnDriversToDistrictsAssignmentReportActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriversToDistrictsAssignmentReport>(),
			() => new QSReport.ReportViewDlg(new DriversToDistrictsAssignmentReport()));
	}

	#endregion Водители

	#region Сервисный центр

	/// <summary>
	/// Отчёт по мастерам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMastersReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MastersReport>(),
			() => new QSReport.ReportViewDlg(new MastersReport(NavigationManager)));
	}

	/// <summary>
	/// Отчёт по оборудованию
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEquipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EquipmentReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport(ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Отчёт по выездам мастеров
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMastersVisitReportActivated(object sender, EventArgs e)
	{
		var employeeFactory = new EmployeeJournalFactory(NavigationManager);
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MastersVisitReport>(),
			() => new QSReport.ReportViewDlg(new MastersVisitReport(employeeFactory)));
	}

	#endregion Сервисный центр

	#region Бухгалтерия

	/// <summary>
	/// Отчет закрытых отгрузок
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCloseDeliveryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyCloseDeliveryReport>(),
			() => new QSReport.ReportViewDlg(new CounterpartyCloseDeliveryReport()));
	}

	/// <summary>
	/// Отчет по оплатам (ФО)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPaymentsFinDepartmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientFinDepartmentReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromBankClientFinDepartmentReport()));
	}

	/// <summary>
	/// Отсрочка сети
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNetworkDelayReportActivated(object sender, EventArgs e)
	{
		ILifetimeScope lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		var employeeJournalFactory = new EmployeeJournalFactory(NavigationManager);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ChainStoreDelayReport>(),
			() => new QSReport.ReportViewDlg(new ChainStoreDelayReport(employeeJournalFactory, lifetimeScope.Resolve<ICounterpartyJournalFactory>(), lifetimeScope.Resolve<Vodovoz.Settings.Counterparty.ICounterpartySettings>())));
	}

	/// <summary>
	/// Отчет по изменениям заказа при доставке
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderChangesReportActivated(object sender, EventArgs e)
	{
		var paramProvider = new ParametersProvider();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderChangesReport>(),
			() => new QSReport.ReportViewDlg(
				new OrderChangesReport(
					new ReportDefaultsProvider(paramProvider),
					ServicesConfig.InteractiveService,
					new ArchiveDataSettings(paramProvider))));
	}

	/// <summary>
	/// Долги по безналу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyCashlessDebtsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyCashlessDebtsReport>(),
			() => new QSReport.ReportViewDlg(_autofacScope.Resolve<CounterpartyCashlessDebtsReport>()));
	}

	/// <summary>
	/// Отчет по УПД в ЧЗ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEdoUpdReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EdoUpdReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Бухгалтерия

	#region Касса

	/// <summary>
	/// По приходу наличных денежных средств
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnIncomeBalanceReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<IncomeBalanceReport>(),
			() => new QSReport.ReportViewDlg(new IncomeBalanceReport()));
	}

	/// <summary>
	/// Зарплаты водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriverWagesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.DriverWagesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.DriverWagesReport(NavigationManager)));
	}

	/// <summary>
	/// Баланс водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriversWageBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriversWageBalanceReport>(),
			() => new QSReport.ReportViewDlg(new DriversWageBalanceReport()));
	}

	/// <summary>
	/// Отчет по выдаче бензина
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelReportActivated(object sender, EventArgs e)
	{
		var scope = Startup.AppDIContainer.BeginLifetimeScope();

		var report = scope.Resolve<Vodovoz.Reports.FuelReport>();

		var tab = tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.FuelReport>(),
			() => new QSReport.ReportViewDlg(report));

		report.Destroyed += (_, _2) => scope?.Dispose();
	}

	/// <summary>
	/// Зарплаты экспедиторов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionForwarderWageReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.ForwarderWageReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.ForwarderWageReport(NavigationManager)));
	}

	/// <summary>
	/// Зарплаты сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionWagesOperationsActivated(object sender, EventArgs e)
	{
		EmployeeFilterViewModel employeeFilter;

		if(_hasAccessToSalariesForLogistics)
		{
			employeeFilter = new EmployeeFilterViewModel(EmployeeCategory.office);
			employeeFilter.SetAndRefilterAtOnce(
				x => x.Category = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking);
		}
		else
		{
			employeeFilter = new EmployeeFilterViewModel();
			employeeFilter.SetAndRefilterAtOnce(x => x.Status = EmployeeStatus.IsWorking);
		}

		employeeFilter.HidenByDefault = true;
		var employeeJournalFactory = new EmployeeJournalFactory(NavigationManager, employeeFilter);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.WagesOperationsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.WagesOperationsReport(employeeJournalFactory)));
	}

	/// <summary>
	/// Кассовая книга
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnCashBoolReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CashBookReport>(),
			() => new QSReport.ReportViewDlg(new CashBookReport(
				new SubdivisionRepository(new ParametersProvider()), ServicesConfig.CommonServices)));
	}

	/// <summary>
	/// Дата ЗП у водителей/экспедиторов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDayOfSalaryGiveoutReport_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DayOfSalaryGiveoutReportViewModel));
	}

	/// <summary>
	/// Отчет по контролю оплаты перемещений
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMovementsPaymentControlReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(MovementsPaymentControlViewModel));
	}

	/// <summary>
	/// Отчет по перемещениям с производств
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnProductionWarehouseMovementReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ProductionWarehouseMovementReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Ставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSalaryRatesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SalaryRatesReport>(),
			() => new QSReport.ReportViewDlg(new SalaryRatesReport(
				UnitOfWorkFactory.GetDefaultFactory,
				new BaseParametersProvider(new ParametersProvider()),
				ServicesConfig.CommonServices)));
	}

	/// <summary>
	/// Налоги сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnEmployeesTaxesActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesTaxesSumReport>(),
			() => new QSReport.ReportViewDlg(new EmployeesTaxesSumReport(UnitOfWorkFactory.GetDefaultFactory)));
	}

	/// <summary>
	/// Анализ движения денежных средств
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction74Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CashFlowAnalysisViewModel>(null);
	}

	#endregion Касса

	#region Производство

	/// <summary>
	/// Отчет по произведенной продукции
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionProducedProductionReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProducedProductionReport>(),
			() => new QSReport.ReportViewDlg(
				new ProducedProductionReport(new NomenclatureJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()))));
	}

	#endregion Производство

	#region Розница

	/// <summary>
	/// Качественный отчет
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionQualityRetailReport(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<QualityReport>(),
			() => new QSReport.ReportViewDlg(new QualityReport(
				new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()),
				new EmployeeJournalFactory(NavigationManager),
				new SalesChannelJournalFactory(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService)));
	}

	/// <summary>
	/// Отчет по контрагентам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyRetailReport(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyReport>(),
			() => new QSReport.ReportViewDlg(new CounterpartyReport(
				new SalesChannelJournalFactory(),
				new DistrictJournalFactory(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService)));

	}

	#endregion Розница

	#region Транспорт

	/// <summary>
	/// Затраты при эксплуатации ТС
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCostCarExploitationReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CostCarExploitationReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Транспорт
}
