using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QS.Project.Services;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class OskOkkReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public OskOkkReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var oskOkkReportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Отчеты");
			var menu = new Menu();
			oskOkkReportsMenuItem.Submenu = menu;

			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по движению бутылей", OnBottlesMovementSummaryReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по движению бутылей (по МЛ)", OnBottlesMovementReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет о несданных бутылях", OnShortfallBottlesPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт \"Куньголово\"", OnReportKungolovoPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по дате создания заказа", OnOrdersByCreationDatePressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по тарифным зонам", OnTariffZoneDebtsReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Реестр МЛ", null/*ActionRLRegister*/));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Клиенты по типам объектов и видам деятельности",
				OnCounterpartyActivityKindPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по пересданной таре водителями", OnExtraBottlesReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по первичным/вторичным заказам", OnFirstSecondReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Рентабельность акции \"Бутыль\"", OnProfitabilityBottlesByStockPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по нулевому долгу клиента", OnZeroDebtClientReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по забору тары", OnReturnedTareReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет о событиях рассылки", OnBulkEmailEventsReportPressed));

			return oskOkkReportsMenuItem;
		}

		/// <summary>
		/// Отчет по движению бутылей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBottlesMovementSummaryReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<BottlesMovementSummaryReport>(),
				() => new QSReport.ReportViewDlg(new BottlesMovementSummaryReport()));
		}

		/// <summary>
		/// Отчет по движению бутылей (по МЛ)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBottlesMovementReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<BottlesMovementReport>(),
				() => new QSReport.ReportViewDlg(new BottlesMovementReport()));
		}

		/// <summary>
		/// Отчет о несданных бутылях
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnShortfallBottlesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ShortfallBattlesReport>(),
				() => new QSReport.ReportViewDlg(new ShortfallBattlesReport(Startup.MainWin.NavigationManager)));
		}

		/// <summary>
		/// Отчёт "Куньголово"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReportKungolovoPressed(object sender, ButtonPressEventArgs e)
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var report = scope.Resolve<ReportForBigClient>();

			report.Destroyed += (s, args) => scope?.Dispose();

			var tab = Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ReportForBigClient>(),
				() => new QSReport.ReportViewDlg(report));
		}

		/// <summary>
		/// Отчет по дате создания заказа
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersByCreationDatePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrdersByCreationDateReport>(),
				() => new QSReport.ReportViewDlg(new OrdersByCreationDateReport()));
		}

		/// <summary>
		/// Отчет по тарифным зонам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTariffZoneDebtsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<TariffZoneDebts>(),
				() => new QSReport.ReportViewDlg(new TariffZoneDebts()));
		}

		/// <summary>
		/// Реестр МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderedByIdRoutesListRegisterPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.RoutesListRegisterReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.RoutesListRegisterReport())
			);
		}

		/// <summary>
		/// Клиенты по типам объектов и видам деятельности
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyActivityKindPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ClientsByDeliveryPointCategoryAndActivityKindsReport>(),
				() => new QSReport.ReportViewDlg(new ClientsByDeliveryPointCategoryAndActivityKindsReport()));
		}

		/// <summary>
		/// Отчет по пересданной таре водителями
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExtraBottlesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ExtraBottleReport>(),
				() => new QSReport.ReportViewDlg(new ExtraBottleReport()));
		}

		/// <summary>
		/// Отчет по первичным/вторичным заказам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFirstSecondReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<FirstSecondClientReport>(),
				() => new QSReport.ReportViewDlg(
					new FirstSecondClientReport(Startup.MainWin.NavigationManager, new DiscountReasonRepository())));
		}

		/// <summary>
		/// Рентабельность акции "Бутыль"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProfitabilityBottlesByStockPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ProfitabilityBottlesByStockReport>(),
				() => new QSReport.ReportViewDlg(new ProfitabilityBottlesByStockReport()));
		}

		/// <summary>
		/// Отчет по нулевому долгу клиента
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnZeroDebtClientReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ZeroDebtClientReport>(),
				() => new QSReport.ReportViewDlg(new ZeroDebtClientReport()));
		}

		/// <summary>
		/// Отчет по забору тары
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReturnedTareReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ReturnedTareReport>(),
				() => new QSReport.ReportViewDlg(new ReturnedTareReport(ServicesConfig.InteractiveService)));
		}

		/// <summary>
		/// Отчет о событиях рассылки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBulkEmailEventsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<BulkEmailEventReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
