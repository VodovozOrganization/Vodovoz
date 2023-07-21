using Autofac;
using QS.Dialog;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QSOrmProject;
using System;
using Vodovoz;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Parameters;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports;

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
}
