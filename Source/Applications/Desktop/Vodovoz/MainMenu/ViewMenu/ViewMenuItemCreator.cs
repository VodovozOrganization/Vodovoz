﻿using System;
using Gtk;

namespace Vodovoz.MainMenu.ViewMenu
{
	public class ViewMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly MainPanelMenuItemHandler _mainPanelMenuItemHandler;
		private readonly TabsMenuItemHandler _tabsMenuItemHandler;
		private readonly ThemesAppMenuItemHandler _themesAppMenuItemHandler;

		public ViewMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			MainPanelMenuItemHandler mainPanelMenuItemHandler,
			TabsMenuItemHandler tabsMenuItemHandler,
			ThemesAppMenuItemHandler themesAppMenuItemHandler)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_mainPanelMenuItemHandler = mainPanelMenuItemHandler ?? throw new ArgumentNullException(nameof(mainPanelMenuItemHandler));
			_tabsMenuItemHandler = tabsMenuItemHandler ?? throw new ArgumentNullException(nameof(tabsMenuItemHandler));
			_themesAppMenuItemHandler = themesAppMenuItemHandler ?? throw new ArgumentNullException(nameof(themesAppMenuItemHandler));
		}
		
		public MenuItem Create()
		{
			var viewMenuItem = _concreteMenuItemCreator.CreateMenuItem("Вид");
			var viewMenu = new Menu();
			viewMenuItem.Submenu = viewMenu;

			viewMenu.Add(_mainPanelMenuItemHandler.Create());
			viewMenu.Add(_tabsMenuItemHandler.Create());
			viewMenu.Add(_themesAppMenuItemHandler.Create());

			return viewMenuItem;
		}
	}
}
