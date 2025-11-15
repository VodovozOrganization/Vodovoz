using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.MainMenu.JournalsMenu.Orders
{
	/// <summary>
	/// Создатель меню Справочники - Заказы - ИПЗ
	/// </summary>
	public class OnlineOrdersSourcesMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private OrdersRatingsMenuItemCreator _ordersRatingsMenuItemCreator;

		public OnlineOrdersSourcesMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			OrdersRatingsMenuItemCreator ordersRatingsMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_ordersRatingsMenuItemCreator = ordersRatingsMenuItemCreator ?? throw new ArgumentNullException(nameof(ordersRatingsMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var onlineOrdersSourcesMenuItem = _concreteMenuItemCreator.CreateMenuItem("ИПЗ");
			var onlineOrdersSourcesMenu = new Menu();
			onlineOrdersSourcesMenuItem.Submenu = onlineOrdersSourcesMenu;
			
			onlineOrdersSourcesMenu.Add(_ordersRatingsMenuItemCreator.Create());
			onlineOrdersSourcesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины отмены онлайн заказов", OnOnlineOrdersCancellationReasonsPressed));
			
			return onlineOrdersSourcesMenuItem;
		}

		/// <summary>
		/// Причины отмены онлайн заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOnlineOrdersCancellationReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OnlineOrderCancellationReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
