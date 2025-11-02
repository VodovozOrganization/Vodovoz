using System;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;

namespace Vodovoz.MainMenu.JournalsMenu.Products
{
	public class InventoryAccountingMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public InventoryAccountingMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var inventoryAccountingMenuItem = _concreteMenuItemCreator.CreateMenuItem("Инвентарный учет");
			var inventoryAccountingMenu = new Menu();
			inventoryAccountingMenuItem.Submenu = inventoryAccountingMenu;
			
			inventoryAccountingMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Экземпляры номенклатур", OnInventoryInstancesPressed));
			inventoryAccountingMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Номенклатуры с инвентарным учетом", OnInventoryNomenclaturesPressed));

			return inventoryAccountingMenuItem;
		}
		
		/// <summary>
		/// Экземпляры номенклатур
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInventoryInstancesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(null);
		}

		/// <summary>
		/// Номенклатуры с инвентарным учетом
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInventoryNomenclaturesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<InventoryNomenclaturesJournalViewModel>(null);
		}
	}
}
