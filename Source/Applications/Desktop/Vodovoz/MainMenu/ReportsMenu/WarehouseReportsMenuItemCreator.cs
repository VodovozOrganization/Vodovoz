using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using QSReport;
using Vodovoz.Presentation.ViewModels.Store.Reports;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.ViewModels.ReportsParameters.Store;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.ViewModels.ViewModels.Warehouses;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Склады
	/// </summary>
	public class WarehouseReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public WarehouseReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
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
			menu.Add(_concreteMenuItemCreator.CreateMenuItem("Оборачиваемость складских остатков", OnTurnoverOfWarehouseBalancesReportPressed));

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
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<EquipmentBalance>().As<IParametersWidget>());
		}

		/// <summary>
		/// Отчёт по браку
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDefectiveItemsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<Vodovoz.ViewModels.Store.Reports.DefectiveItemsReportViewModel>(null);
		}

		/// <summary>
		/// Недопогруженные МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNotFullyLoadedRouteListsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotFullyLoadedRouteListsReportViewModel));
		}

		/// <summary>
		/// Товары для отгрузки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnForShipmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<NomenclatureForShipment>().As<IParametersWidget>());
		}

		/// <summary>
		/// Развернутые движения ТМЦ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStockMovementsAdvancedReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<StockMovementsAdvancedReport>().As<IParametersWidget>());
		}

		/// <summary>
		/// Заявка на производство
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProductionRequestReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				options: OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<ProductionRequestReport>().As<IParametersWidget>());
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
		
		/// <summary>
		/// Оборачиваемость складских остатков
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTurnoverOfWarehouseBalancesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<TurnoverOfWarehouseBalancesReportViewModel>(null);
		}
	}
}
