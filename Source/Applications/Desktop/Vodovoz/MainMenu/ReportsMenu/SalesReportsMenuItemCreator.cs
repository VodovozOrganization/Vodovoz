using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using QSReport;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class SalesReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _orderCreationDateMenuItem;
		private MenuItem _planImplementationMenuItem;
		private MenuItem _setBillsMenuItem;

		public SalesReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create(bool userIsSalesRepresentative)
		{
			var salesReportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Продажи");
			var salesReportsMenu = new Menu();
			salesReportsMenuItem.Submenu = salesReportsMenu;
			
			salesReportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по продажам", OnSalesReportPressed));

			_orderCreationDateMenuItem = _concreteMenuItemCreator.CreateMenuItem(
				"Отчет по дате создания заказа", OnOrderCreationDateReportPressed);
			salesReportsMenu.Add(_orderCreationDateMenuItem);
			_orderCreationDateMenuItem.Visible = !userIsSalesRepresentative;

			_planImplementationMenuItem = _concreteMenuItemCreator.CreateMenuItem(
				"Отчёт о выполнении плана", OnPlanImplementationReportPressed);
			salesReportsMenu.Add(_planImplementationMenuItem);
			_planImplementationMenuItem.Visible = !userIsSalesRepresentative;

			_setBillsMenuItem = _concreteMenuItemCreator.CreateMenuItem(
				"Отчет по выставленным счетам", OnSetBillsReportPressed);
			salesReportsMenu.Add(_setBillsMenuItem);
			_setBillsMenuItem.Visible = !userIsSalesRepresentative;
			
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
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrderCreationDateReportViewModel));
		}

		/// <summary>
		/// Отчёт о выполнении плана
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlanImplementationReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<PlanImplementationReport>().As<IParametersWidget>());
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
