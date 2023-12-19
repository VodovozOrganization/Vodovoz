using System;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class SalesReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public SalesReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var salesReportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Продажи");
			var salesReportsMenu = new Menu();
			salesReportsMenuItem.Submenu = salesReportsMenu;

			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по продажам", OnSalesReportPressed));
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по дате создания заказа", OnOrderCreationDateReportPressed));
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчёт о выполнении плана", OnPlanImplementationReportPressed));
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по выставленным счетам", OnSetBillsReportPressed));
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по продажам с рентабельностью", OnProfitabilitySalesReportPressed));
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по оборачиваемости с динамикой", OnTurnoverWithDynamicsReportPressed));
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Аналитика продаж КБ", OnSalesBySubdivisionsAnalyticsPressed));

			return salesReportsMenuItem;
		}

		/// <summary>
		/// Отчет по продажам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(SalesReportViewModel));
		}

		/// <summary>
		/// Отчет по дате создания заказа
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderCreationDateReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrderCreationDateReport>(),
				() => new QSReport.ReportViewDlg(new OrderCreationDateReport(Startup.MainWin.NavigationManager)));
		}

		/// <summary>
		/// Отчёт о выполнении плана
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlanImplementationReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<PlanImplementationReport>(),
				() => new QSReport.ReportViewDlg(new PlanImplementationReport()));
		}

		/// <summary>
		/// Отчет по выставленным счетам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSetBillsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(SetBillsReportViewModel));
		}

		/// <summary>
		/// Отчет по продажам с рентабельностью
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProfitabilitySalesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProfitabilitySalesReportViewModel));
		}

		/// <summary>
		/// Отчет по оборачиваемости с динамикой
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTurnoverWithDynamicsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<TurnoverWithDynamicsReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Аналитика продаж КБ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalesBySubdivisionsAnalyticsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<SalesBySubdivisionsAnalitycsReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
