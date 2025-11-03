using System;
using Autofac;
using Gtk;
using QS.Dialog;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QSReport;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ViewModels.Reports.OKS.DailyReport;
using Vodovoz.ViewModels.ReportsParameters.Bottles;
using Vodovoz.ViewModels.ReportsParameters.Client;
using Vodovoz.ViewModels.ReportsParameters.Logistics;
using Vodovoz.ViewModels.ReportsParameters.Orders;
using Vodovoz.ViewModels.ReportsParameters.QualityControl;
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
			var oskOkkReportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Отчеты ОСК/ОКК");
			var menu = new Menu();
			oskOkkReportsMenuItem.Submenu = menu;

			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по движению бутылей", OnBottlesMovementSummaryReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по движению бутылей (по МЛ)", OnBottlesMovementReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет о несданных бутылях", OnShortfallBottlesPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт \"Куньголово\"", OnReportKungolovoPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по дате создания заказа", OnOrdersByCreationDatePressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по тарифным зонам", OnTariffZoneDebtsReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Реестр МЛ", OnOrderedByIdRoutesListRegisterPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Клиенты по типам объектов и видам деятельности",
				OnCounterpartyActivityKindPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по пересданной таре водителями", OnExtraBottlesReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по первичным/вторичным заказам", OnFirstSecondReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Рентабельность акции \"Бутыль\"", OnProfitabilityBottlesByStockPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по нулевому долгу клиента", OnZeroDebtClientReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по забору тары", OnReturnedTareReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет о событиях рассылки", OnBulkEmailEventsReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Ежедневный отчет ОКС", OnOksDailyReportPressed));

			return oskOkkReportsMenuItem;
		}

		/// <summary>
		/// Отчет по движению бутылей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBottlesMovementSummaryReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(BottlesMovementSummaryReportViewModel));
		}

		/// <summary>
		/// Отчет по движению бутылей (по МЛ)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBottlesMovementReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(BottlesMovementReportViewModel));
		}

		/// <summary>
		/// Отчет о несданных бутылях
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnShortfallBottlesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ShortfallBattlesReportViewModel));
		}

		/// <summary>
		/// Отчёт "Куньголово"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReportKungolovoPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				options: OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<ReportForBigClient>().As<IParametersWidget>());
		}

		/// <summary>
		/// Отчет по дате создания заказа
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersByCreationDatePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrdersByCreationDateReportViewModel));
		}

		/// <summary>
		/// Отчет по тарифным зонам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTariffZoneDebtsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(TariffZoneDebtsViewModel));
		}

		/// <summary>
		/// Реестр МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderedByIdRoutesListRegisterPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(RoutesListRegisterReportViewModel));
		}

		/// <summary>
		/// Клиенты по типам объектов и видам деятельности
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyActivityKindPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin
				.NavigationManager
				.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel));
		}

		/// <summary>
		/// Отчет по пересданной таре водителями
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExtraBottlesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ExtraBottleReportViewModel));
		}

		/// <summary>
		/// Отчет по первичным/вторичным заказам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFirstSecondReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin
				.NavigationManager
				.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FirstSecondClientReportViewModel), OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Рентабельность акции "Бутыль"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProfitabilityBottlesByStockPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProfitabilityBottlesByStockReportViewModel));
		}

		/// <summary>
		/// Отчет по нулевому долгу клиента
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnZeroDebtClientReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ZeroDebtClientReportViewModel));
		}

		/// <summary>
		/// Отчет по забору тары
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReturnedTareReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<ReturnedTareReport>().As<IParametersWidget>());
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

		/// <summary>
		/// Ежедневный отчет ОКС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOksDailyReportPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OksDailyReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
