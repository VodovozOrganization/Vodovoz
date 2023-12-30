using System;
using Gtk;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.ViewModels.ViewModels.Warehouses;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class WarehouseReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public WarehouseReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var warehouseReportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Склады");
			var menu = new Menu();
			warehouseReportsMenuItem.Submenu = menu;

			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Остатки", OnWarehousesBalanceSummaryReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem(Startup.MainWin.StockMovementsAction));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("ТМЦ на остатках", OnEquipmentBalancePressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по браку", OnDefectiveItemsReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Недопогруженные МЛ", OnNotFullyLoadedRouteListsPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Товары для отгрузки", OnForShipmentReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Развернутые движения ТМЦ", OnStockMovementsAdvancedReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Заявка на производство", OnProductionRequestReportPressed));
			menu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Движение по инвентарному номеру", OnInventoryInstanceMovementReportPressed));

			return warehouseReportsMenuItem;
		}

		/// <summary>
		/// Остатки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWarehousesBalanceSummaryReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<WarehousesBalanceSummaryViewModel>(null);
		}

		/// <summary>
		/// ТМЦ на остатках
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEquipmentBalancePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<EquipmentBalance>(),
				() => new QSReport.ReportViewDlg(new EquipmentBalance()));
		}

		/// <summary>
		/// Отчёт по браку
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDefectiveItemsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<DefectiveItemsReport>(),
				() => new QSReport.ReportViewDlg(new DefectiveItemsReport(Startup.MainWin.NavigationManager)));
		}

		/// <summary>
		/// Недопогруженные МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNotFullyLoadedRouteListsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<NotFullyLoadedRouteListsReport>(),
				() => new QSReport.ReportViewDlg(new NotFullyLoadedRouteListsReport()));
		}

		/// <summary>
		/// Товары для отгрузки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnForShipmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<NomenclatureForShipment>(),
				() => new QSReport.ReportViewDlg(new NomenclatureForShipment()));
		}

		/// <summary>
		/// Развернутые движения ТМЦ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStockMovementsAdvancedReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<StockMovementsAdvancedReport>(),
				() => new QSReport.ReportViewDlg(new StockMovementsAdvancedReport()));
		}

		/// <summary>
		/// Заявка на производство
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProductionRequestReportPressed(object sender, ButtonPressEventArgs e)
		{
			var employeeRepository = new EmployeeRepository();

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ProductionRequestReport>(),
				() => new QSReport.ReportViewDlg(new ProductionRequestReport(employeeRepository)));
		}

		/// <summary>
		/// Движение по инвентарному номеру
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInventoryInstanceMovementReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<InventoryInstanceMovementReportViewModel>(null);
		}
	}
}
