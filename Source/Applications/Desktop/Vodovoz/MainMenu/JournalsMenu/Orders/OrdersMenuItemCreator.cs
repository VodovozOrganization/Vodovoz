using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.MainMenu.JournalsMenu.Orders
{
	public class OrdersMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly OnlineOrdersSourcesMenuItemCreator _onlineOrdersSourcesMenuItemCreator;

		public OrdersMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			OnlineOrdersSourcesMenuItemCreator onlineOrdersSourcesMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_onlineOrdersSourcesMenuItemCreator =
				onlineOrdersSourcesMenuItemCreator ?? throw new ArgumentNullException(nameof(onlineOrdersSourcesMenuItemCreator));
		}

		public MenuItem Create()
		{
			var ordersMenuItem = _concreteMenuItemCreator.CreateMenuItem("Заказы");
			var ordersMenu = new Menu();
			ordersMenuItem.Submenu = ordersMenu;
		
			ordersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Типы оплаты по карте", OnPaymentsFromPressed));
			ordersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("План продаж для КЦ", OnSalesPlanPressed));
			ordersMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Причины отсутствия переноса", OnUndeliveryTransferAbsenceReasonsPressed));
			ordersMenu.Add(_onlineOrdersSourcesMenuItemCreator.Create());

			return ordersMenuItem;
		}

		/// <summary>
		/// Типы оплаты по карте
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaymentsFromPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<PaymentsFromJournalViewModel>(null);
		}

		/// <summary>
		/// План продаж для КЦ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalesPlanPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager
				.OpenViewModel<NomenclaturesPlanJournalViewModel, Action<NomenclaturePlanFilterViewModel>>(
					null,
					filter => filter.HidenByDefault = true);
		}

		/// <summary>
		/// Причины отсутствия переноса
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUndeliveryTransferAbsenceReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager
				.OpenViewModel<UndeliveryTransferAbsenceReasonJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
