using System;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.MainMenu.JournalsMenu.Products
{
	public class ExternalSourcesMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly ExternalSourceCatalogsMenuItemCreator _externalSourceCatalogsMenuItemCreator;

		public ExternalSourcesMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			ExternalSourceCatalogsMenuItemCreator externalSourceCatalogsMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_externalSourceCatalogsMenuItemCreator =
				externalSourceCatalogsMenuItemCreator ?? throw new ArgumentNullException(nameof(externalSourceCatalogsMenuItemCreator));
		}

		public MenuItem Create()
		{
			var externalSourcesMenuItem = _concreteMenuItemCreator.CreateMenuItem("ИПЗ");
			var externalSourcesMenu = new Menu();
			externalSourcesMenuItem.Submenu = externalSourcesMenu;
			
			externalSourcesMenu.Add(_externalSourceCatalogsMenuItemCreator.Create());
			externalSourcesMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Группы товаров в ИПЗ", OnNomenclatureOnlineGroupsPressed));
			externalSourcesMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Типы товаров в ИПЗ", OnNomenclatureOnlineCategoriesPressed));

			return externalSourcesMenuItem;
		}
		
		/// <summary>
		/// ИПЗ - Группы товаров в ИПЗ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNomenclatureOnlineGroupsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<NomenclatureOnlineGroupsJournalViewModel>(null);
		}

		/// <summary>
		/// ИПЗ - Типы товаров в ИПЗ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNomenclatureOnlineCategoriesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<NomenclatureOnlineCategoriesJournalViewModel>(null);
		}
	}
}
