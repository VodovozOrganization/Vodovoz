using System;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ReportsParameters.Payments;
using Vodovoz.ViewModels.ViewModels.Reports;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class OrderReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public OrderReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var orderReportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Заказы");
			var orderReportsMenu = new Menu();
			orderReportsMenuItem.Submenu = orderReportsMenu;

			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по комментариям кассы", OnCashierCommentsPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по районам", OnOrdersByDistrictPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по последнему заказу", OnLastOrderReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по некорректным ценам", OnOrderIncorrectPricesReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчёт по заказам меньше 100 р.", OnOrdersWithMinPriceLessThanPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по самовывозу", OnSelfDeliveryReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по ценам пригорода", OnSuburbWaterPricePressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Теги контрагентов", OnCounterpartyTagsPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по недовозам", OnNotDeliveredOrdersPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Реестр заказов", OnOrderRegistryPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Оплата по картам", OnCardPaymentsPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Оплаты OnLine заказов", OnPayOnLineOrdersPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Первичные клиенты", OnFirstClientsPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по скидкам", OnSalesByDiscountReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по потенциальным халявщикам", OnPotentialFreeloadersReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по оплатам", OnPaymentsReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по заказам ИМ", OnEShopSalesReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Аналитика заказов", OnOrderAnalyticsReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по мотивации КЦ", OnNomenclaturePlanReportPressed));
			orderReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Оплаты по СБП из Авангарда", OnPaymentsFromAvangardReportPressed));

			return orderReportsMenuItem;
		}

		/// <summary>
		/// Отчет по комментариям кассы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCashierCommentsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.CashierCommentsReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.CashierCommentsReport()));
		}

		/// <summary>
		/// Отчет по районам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersByDistrictPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictReport>(),
				() => new QSReport.ReportViewDlg(new OrdersByDistrictReport()));
		}

		/// <summary>
		/// Отчет по последнему заказу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLastOrderReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<LastOrderByDeliveryPointReport>(),
				() => new QSReport.ReportViewDlg(new LastOrderByDeliveryPointReport()));
		}

		/// <summary>
		/// Отчет по некорректным ценам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderIncorrectPricesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrderIncorrectPrices>(),
				() => new QSReport.ReportViewDlg(new OrderIncorrectPrices()));
		}

		/// <summary>
		/// Отчёт по заказам меньше 100 р.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersWithMinPriceLessThanPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrdersWithMinPriceLessThan>(),
				() => new QSReport.ReportViewDlg(new OrdersWithMinPriceLessThan()));
		}

		/// <summary>
		/// Отчёт по самовывозу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSelfDeliveryReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<SelfDeliveryReport>(),
				() => new QSReport.ReportViewDlg(new SelfDeliveryReport()));
		}

		/// <summary>
		/// Отчет по ценам пригорода
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSuburbWaterPricePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<SuburbWaterPriceReport>(),
				() => new QSReport.ReportViewDlg(new SuburbWaterPriceReport()));
		}

		/// <summary>
		/// Теги контрагентов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyTagsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<TagJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Отчет по недовозам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNotDeliveredOrdersPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<NotDeliveredOrdersReport>(),
				() => new QSReport.ReportViewDlg(new NotDeliveredOrdersReport()));
		}

		/// <summary>
		/// Реестр заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderRegistryPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrderRegistryReport>(),
				() => new QSReport.ReportViewDlg(new OrderRegistryReport()));
		}

		/// <summary>
		/// Оплата по картам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCardPaymentsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CardPaymentsOrdersReport>(),
				() => new QSReport.ReportViewDlg(new CardPaymentsOrdersReport(UnitOfWorkFactory.GetDefaultFactory)));
		}

		/// <summary>
		/// Оплаты OnLine заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPayOnLineOrdersPressed(object sender, ButtonPressEventArgs e)
		{
			var paymentsRepository = new PaymentsRepository();

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<PaymentsFromTinkoffReport>(),
				() => new QSReport.ReportViewDlg(new PaymentsFromTinkoffReport(paymentsRepository)));
		}

		/// <summary>
		/// Первичные клиенты
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFirstClientsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
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
		private void OnSalesByDiscountReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<SalesByDiscountReport>(),
				() => new QSReport.ReportViewDlg(new SalesByDiscountReport()));
		}

		/// <summary>
		/// Отчет по потенциальным халявщикам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPotentialFreeloadersReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<PotentialFreePromosetsReport>(),
				() => new QSReport.ReportViewDlg(new PotentialFreePromosetsReport()));
		}

		/// <summary>
		/// Отчет по оплатам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaymentsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(PaymentsFromBankClientReportViewModel));
		}

		/// <summary>
		/// Отчет по заказам ИМ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEShopSalesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<EShopSalesReport>(),
				() => new QSReport.ReportViewDlg(new EShopSalesReport()));
		}

		/// <summary>
		/// Аналитика заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderAnalyticsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OrderAnalyticsReportViewModel>(null);
		}

		/// <summary>
		/// Отчет по мотивации КЦ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNomenclaturePlanReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<NomenclaturePlanReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Оплаты из Авангарда
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaymentsFromAvangardReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<PaymentsFromAvangardReport>(),
				() => new QSReport.ReportViewDlg(new PaymentsFromAvangardReport()));
		}
	}
}
