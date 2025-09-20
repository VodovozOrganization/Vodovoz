using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QSReport;
using System;
using Vodovoz;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;
using Vodovoz.Presentation.ViewModels.Store.Reports;
using Vodovoz.Reports;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Employees;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Retail;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Reports;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges;
using Vodovoz.ViewModels.Bookkeepping.Reports.EdoControl;
using Vodovoz.ViewModels.Cash.Reports;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Orders.Reports;
using Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Reports.OKS.DailyReport;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewModels.ReportsParameters.Bookkeeping;
using Vodovoz.ViewModels.ReportsParameters.Bottles;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using Vodovoz.ViewModels.ReportsParameters.Client;
using Vodovoz.ViewModels.ReportsParameters.Fuel;
using Vodovoz.ViewModels.ReportsParameters.Logistic;
using Vodovoz.ViewModels.ReportsParameters.Logistic.CarOwnershipReport;
using Vodovoz.ViewModels.ReportsParameters.Logistics;
using Vodovoz.ViewModels.ReportsParameters.Orders;
using Vodovoz.ViewModels.ReportsParameters.Payments;
using Vodovoz.ViewModels.ReportsParameters.Production;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.ReportsParameters.QualityControl;
using Vodovoz.ViewModels.ReportsParameters.Sales;
using Vodovoz.ViewModels.ReportsParameters.Selfdelivery;
using Vodovoz.ViewModels.ReportsParameters.Service;
using Vodovoz.ViewModels.ReportsParameters.Store;
using Vodovoz.ViewModels.ReportsParameters.Wages;
using Vodovoz.ViewModels.Transport.Reports.IncorrectFuel;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.ViewModels.Reports.Cars.ExploitationReport;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport;
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CashierCommentsReportViewModel));
	}

	/// <summary>
	/// Отчет по районам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersByDistrict(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrdersByDistrictReportViewModel));
	}

	/// <summary>
	/// Отчет по последнему заказу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionLastOrderReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(LastOrderByDeliveryPointReportViewModel));
	}

	/// <summary>
	/// Отчет по некорректным ценам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderIncorrectPricesReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrderIncorrectPricesViewModel));
	}

	/// <summary>
	/// Отчёт по заказам меньше 100 р.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersWithMinPriceLessThanActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrdersWithMinPriceLessThanViewModel));
	}

	/// <summary>
	/// Отчёт по самовывозу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSelfDeliveryReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(SelfDeliveryReportViewModel));
	}

	/// <summary>
	/// Отчет по ценам пригорода
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSuburbWaterPriceActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(SuburbWaterPriceReportViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotDeliveredOrdersReportViewModel));
	}

	/// <summary>
	/// Реестр заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderRegistryActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrderRegistryReportViewModel));
	}

	/// <summary>
	/// Оплата по картам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCardPaymentsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CardPaymentsOrdersReportViewModel));
	}

	/// <summary>
	/// Оплаты OnLine заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnOnLineActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OnlinePaymentsReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Первичные клиенты
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFirstClientsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FirstClientsReportViewModel), OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Отчет по скидкам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSalesByDicountReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SalesByDiscountReport>(),
			() => new QSReport.ReportViewDlg(new SalesByDiscountReport(reportInfoFactory)));
	}

	/// <summary>
	/// Отчет по потенциальным халявщикам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction66Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PotentialFreePromosetsReportViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(EShopSalesReportViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(PaymentsFromAvangardReportViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrderCreationDateReportViewModel));
	}

	/// <summary>
	/// Отчёт о выполнении плана
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPlanImplementationReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PlanImplementationReport>(),
			() => new QSReport.ReportViewDlg(new PlanImplementationReport(reportInfoFactory)));
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
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		var report = new Vodovoz.Reports.StockMovements(reportInfoFactory, NavigationManager, Startup.AppDIContainer.BeginLifetimeScope());
		var dlg = new QSReport.ReportViewDlg(report);
		report.ParentTab = dlg;

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.StockMovements>(),
			() => dlg);
	}

	/// <summary>
	/// ТМЦ на остатках
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEquipmentBalanceActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EquipmentBalance>(),
			() => new QSReport.ReportViewDlg(new EquipmentBalance(reportInfoFactory)));
	}

	/// <summary>
	/// Отчёт по браку
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDefectiveItemsReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<Vodovoz.ViewModels.Store.Reports.DefectiveItemsReportViewModel>(null);
	}

	/// <summary>
	/// Недопогруженные МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNotFullyLoadedRouteListsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotFullyLoadedRouteListsReportViewModel));
	}

	/// <summary>
	/// Товары для отгрузки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnForShipmentReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			options: OpenPageOptions.IgnoreHash,
			addingRegistrations: builder => builder.RegisterType<NomenclatureForShipment>().As<IParametersWidget>());
	}

	/// <summary>
	/// Развернутые движения ТМЦ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionStockMovementsAdvancedReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<StockMovementsAdvancedReport>(),
			() => new QSReport.ReportViewDlg(new StockMovementsAdvancedReport(reportInfoFactory)));
	}

	/// <summary>
	/// Заявка на производство
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionProductionRequestReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			options: OpenPageOptions.IgnoreHash,
			addingRegistrations: builder => builder.RegisterType<ProductionRequestReport>().As<IParametersWidget>());
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

	/// <summary>
	/// Оборачиваемость складских остатков
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction90Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<TurnoverOfWarehouseBalancesReportViewModel>(null);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(BottlesMovementSummaryReportViewModel));
	}

	/// <summary>
	/// Отчет по движению бутылей (по МЛ)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionBottlesMovementReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(BottlesMovementReportViewModel));
	}

	/// <summary>
	/// Отчет о несданных бутылях
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionShortfallBattlesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ShortfallBattlesReportViewModel));
	}

	/// <summary>
	/// Отчёт "Куньголово"
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnReportKungolovoActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			options: OpenPageOptions.IgnoreHash,
			addingRegistrations: builder => builder.RegisterType<ReportForBigClient>().As<IParametersWidget>());
	}

	/// <summary>
	/// Отчет по дате создания заказа
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersByCreationDate(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrdersByCreationDateReportViewModel));
	}

	/// <summary>
	/// Отчет по тарифным зонам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionTariffZoneDebtsReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(TariffZoneDebtsViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel));
	}

	/// <summary>
	/// Отчет по пересданной таре водителями
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionExtraBottlesReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ExtraBottleReportViewModel));
	}

	/// <summary>
	/// Отчет по первичным/вторичным заказам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFirstSecondReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FirstSecondClientReportViewModel), OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Рентабельность акции "Бутыль"
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionProfitabilityBottlesByStockActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProfitabilityBottlesByStockReportViewModel));
	}

	/// <summary>
	/// Отчет по нулевому долгу клиента
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionZeroDebtClientReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ZeroDebtClientReportViewModel));
	}

	/// <summary>
	/// Отчет по забору тары
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionReturnedTareReportActivated(object sender, EventArgs e)
	{
		var interactiveService = _autofacScope.Resolve<IInteractiveService>();
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReturnedTareReport>(),
			() => new QSReport.ReportViewDlg(new ReturnedTareReport(reportInfoFactory, interactiveService)));
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

	/// <summary>
	/// Ежедневный отчет ОКС
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOksDailyReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OksDailyReportViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrdersByDistrictsAndDeliverySchedulesReportViewModel));
	}

	/// <summary>
	/// Отчет по выдаче топлива по МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelConsumptionReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FuelConsumptionReportViewModel));
	}

	/// <summary>
	/// Отчет по времени приема заказов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersCreationTimeReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersCreationTimeReport>(),
			() => new QSReport.ReportViewDlg(new OrdersCreationTimeReport(reportInfoFactory)));
	}

	/// <summary>
	/// Путевой лист
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionWayBillReportActivated(object sender, EventArgs e)
	 {
		var dlg = NavigationManager.OpenTdiTab<ReportViewDlg>(
		null,
		options: OpenPageOptions.IgnoreHash,
		addingRegistrations: builder => builder.RegisterType<WayBillReportGroupPrint>().As<IParametersWidget>())
		.TdiTab;

		var report = (dlg as ReportViewDlg).ParametersWidget;
		(report as WayBillReportGroupPrint).ParentTab = dlg ;
	}

	/// <summary>
	/// Отчет по незакрытым МЛ за период
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNonClosedRLByPeriodReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NonClosedRLByPeriodReportViewModel));
	}

	/// <summary>
	/// График выхода на линию за смену
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionScheduleOnLinePerShiftReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ScheduleOnLinePerShiftReportViewModel));
	}

	/// <summary>
	/// Статистика по дням недели
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderStatisticByWeekReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderStatisticByWeekReport>(),
			() => new QSReport.ReportViewDlg(new OrderStatisticByWeekReport(reportInfoFactory, ServicesConfig.InteractiveService)));
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
		NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			addingRegistrations: builder =>
			{
				builder.Register(c => new EmployeeFilterViewModel
				{
					Category = EmployeeCategory.driver
				});
				builder.RegisterType<GeneralSalaryInfoReport>().As<IParametersWidget>();
			});
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
		NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			addingRegistrations: builder =>
			{
				builder.Register(c => new EmployeeFilterViewModel
				{
					RestrictCategory = EmployeeCategory.driver
				});
				builder.RegisterType<AddressesOverpaymentsReport>().As<IParametersWidget>();
			});
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(AnalyticsForUndeliveryReportViewModel));
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
	
	/// <summary>
	/// Отчет по событиям нахождения волителей на складе
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnDriversWarehousesEventsReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DriversWarehousesEventsReportViewModel>(null);
	}

	/// <summary>
	/// Отчет о принадлежности ТС
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnCarOwnershipReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CarOwnershipReportViewModel));
	}

	/// <summary>
	/// Отчет по изменению формы оплаты водителями
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnChangingFormOfPaymentbyDriversReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ChangingPaymentTypeByDriversReportViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(EmployeesFinesViewModel));
	}

	/// <summary>
	/// Премии сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction48Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(EmployeesPremiumsViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DeliveriesLateReportViewModel));
	}

	/// <summary>
	/// Отчет по незакрытым МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRouteListsOnClosingActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(RouteListsOnClosingReportViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OnLoadTimeAtDayReportViewModel));
	}

	/// <summary>
	/// Время доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveryTimeReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DeliveryTimeReportViewModel));
	}

	/// <summary>
	/// Загрузка наших автомобилей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCompanyTrucksActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CompanyTrucksReportViewModel));
	}

	/// <summary>
	/// Отчёт по отгрузке автомобилей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionShipmentReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ShipmentReportViewModel));
	}

	/// <summary>
	/// Отчёт по километражу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMileageReportActivated(object sender, EventArgs e)
	{
		var dlg = NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			options: OpenPageOptions.IgnoreHash,
			addingRegistrations: builder => builder.RegisterType<MileageReport>().As<IParametersWidget>())
			.TdiTab;
		
		var report = (dlg as ReportViewDlg).ParametersWidget;
		(report as MileageReport).ParentTab = dlg;
	}

	/// <summary>
	/// Отчёт по водительскому телефону
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriveingCallsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DrivingCallReportViewModel));
	}

	/// <summary>
	/// Отчет по распределению водителей на районы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnDriversToDistrictsAssignmentReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DriversToDistrictsAssignmentReportViewModel));
	}

	/// <summary>
	/// Отчет по последнему МЛ по водителям
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnLastRouteListrReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<LastRouteListReportViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(MastersReportViewModel));
	}

	/// <summary>
	/// Отчёт по оборудованию
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEquipmentReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		var interactiveService = _autofacScope.Resolve<IInteractiveService>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EquipmentReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport(reportInfoFactory, interactiveService)));
	}

	/// <summary>
	/// Отчёт по выездам мастеров
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMastersVisitReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(MastersVisitReportViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CounterpartyCloseDeliveryReportViewModel));
	}

	/// <summary>
	/// Отчет по оплатам (ФО)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPaymentsFinDepartmentReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(PaymentsFromBankClientFinDepartmentReportViewModel));
	}

	/// <summary>
	/// Отсрочка сети
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNetworkDelayReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ChainStoreDelayReportViewModel), OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Отчет по изменениям заказа при доставке
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrderChangesReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrderChangesReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Долги по безналу
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyCashlessDebtsReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CounterpartyCashlessDebtsReportViewModel));
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

	/// <summary>
	/// Контроль за ЭДО
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEdoControlReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EdoControlReportViewModel>(null, OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(IncomeBalanceReportViewModel));
	}

	/// <summary>
	/// Зарплаты водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriverWagesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DriverWagesReportViewModel));
	}

	/// <summary>
	/// Баланс водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriversWageBalanceActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriversWageBalanceReport>(),
			() => new QSReport.ReportViewDlg(new DriversWageBalanceReport(reportInfoFactory)));
	}

	/// <summary>
	/// Отчет по выдаче бензина
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelReportActivated(object sender, EventArgs e)
	{
		var dlg = NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			options: OpenPageOptions.IgnoreHash,
			addingRegistrations: builder => builder.RegisterType<FuelReport>().As<IParametersWidget>())
			.TdiTab;
		
		var report = (dlg as ReportViewDlg).ParametersWidget;
		(report as FuelReport).ParentTab = dlg;
	}

	/// <summary>
	/// Зарплаты экспедиторов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionForwarderWageReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ForwarderWageReportViewModel));
	}

	/// <summary>
	/// Зарплаты сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionWagesOperationsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(WagesOperationsReportViewModel));
	}

	/// <summary>
	/// Кассовая книга
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnCashBoolReportActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		var commonServices = _autofacScope.Resolve<ICommonServices>();
		ISubdivisionRepository subdivisionRepository = _autofacScope.Resolve<ISubdivisionRepository>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CashBookReport>(),
			() => new QSReport.ReportViewDlg(new CashBookReport(reportInfoFactory, subdivisionRepository, commonServices)));
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
		var uowFactory = _autofacScope.Resolve<IUnitOfWorkFactory>();
		var wageSettings = _autofacScope.Resolve<IWageSettings>();
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SalaryRatesReport>(),
			() => new QSReport.ReportViewDlg(new SalaryRatesReport(
				reportInfoFactory,
				uowFactory,
				wageSettings,
				ServicesConfig.CommonServices)));
	}

	/// <summary>
	/// Налоги сотрудников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnEmployeesTaxesActionActivated(object sender, EventArgs e)
	{
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesTaxesSumReport>(),
			() => new QSReport.ReportViewDlg(new EmployeesTaxesSumReport(reportInfoFactory, ServicesConfig.UnitOfWorkFactory)));
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

	/// <summary>
	/// Отчет по запросам к API Газпром-нефть
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelApiRequestReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FuelApiRequestReportViewModel));
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
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProducedProductionReportViewModel));
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
		NavigationManager.OpenTdiTab<ReportViewDlg>(
			null,
			addingRegistrations: builder => builder.RegisterType<QualityReport>().As<IParametersWidget>());
	}

	/// <summary>
	/// Отчет по контрагентам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyRetailReport(object sender, EventArgs e)
	{
		var interactiveService = _autofacScope.Resolve<IInteractiveService>();
		var reportInfoFactory = _autofacScope.Resolve<IReportInfoFactory>();
		var uowFactory = _autofacScope.Resolve<IUnitOfWorkFactory>();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyReport>(),
			() => new QSReport.ReportViewDlg(new CounterpartyReport(
				reportInfoFactory,
				new SalesChannelJournalFactory(),
				_autofacScope.Resolve<IDistrictJournalFactory>(),
				uowFactory,
				interactiveService)));

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

	/// <summary>
	/// Отчёт по простою
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction89Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarIsNotAtLineReportParametersViewModel>(null);
	}

	/// <summary>
	/// Отчет по расходу топлива
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionAverageFlowDiscrepancyReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<AverageFlowDiscrepanciesReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Отчет по заправкам некорректным типом топлива
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionIncorrectFuelReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<IncorrectFuelReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Транспорт
}
