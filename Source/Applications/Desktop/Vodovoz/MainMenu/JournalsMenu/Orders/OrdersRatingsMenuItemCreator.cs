using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.MainMenu.JournalsMenu.Orders
{
	public class OrdersRatingsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public OrdersRatingsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var ordersRatingsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Оценки заказов");
			var ordersRatingsMenu = new Menu();
			ordersRatingsMenuItem.Submenu = ordersRatingsMenu;
			
			ordersRatingsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины оценки заказов", OnOrdersRatingReasonsPressed));
			
			return ordersRatingsMenuItem;
		}
		
		/// <summary>
		/// Причины оценки заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersRatingReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OrdersRatingReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
