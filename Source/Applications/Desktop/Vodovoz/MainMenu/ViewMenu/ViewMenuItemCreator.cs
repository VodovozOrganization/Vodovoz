using System;
using Gtk;

namespace Vodovoz.MainMenu.ViewMenu
{
	/// <summary>
	/// Создатель меню Вид
	/// </summary>
	public class ViewMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly TabsMenuItemHandler _tabsMenuItemHandler;
		private readonly ThemesAppMenuItemHandler _themesAppMenuItemHandler;

		public ViewMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			MainPanelMenuItemHandler mainPanelMenuItemHandler,
			TabsMenuItemHandler tabsMenuItemHandler,
			ThemesAppMenuItemHandler themesAppMenuItemHandler)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			MainPanelMenuItemHandler = mainPanelMenuItemHandler ?? throw new ArgumentNullException(nameof(mainPanelMenuItemHandler));
			_tabsMenuItemHandler = tabsMenuItemHandler ?? throw new ArgumentNullException(nameof(tabsMenuItemHandler));
			_themesAppMenuItemHandler = themesAppMenuItemHandler ?? throw new ArgumentNullException(nameof(themesAppMenuItemHandler));
		}

		public MainPanelMenuItemHandler MainPanelMenuItemHandler { get; }

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var viewMenuItem = _concreteMenuItemCreator.CreateMenuItem("Вид");
			var viewMenu = new Menu();
			viewMenuItem.Submenu = viewMenu;

			viewMenu.Add(MainPanelMenuItemHandler.Create());
			viewMenu.Add(_tabsMenuItemHandler.Create());
			viewMenu.Add(_themesAppMenuItemHandler.Create());

			return viewMenuItem;
		}
	}
}
