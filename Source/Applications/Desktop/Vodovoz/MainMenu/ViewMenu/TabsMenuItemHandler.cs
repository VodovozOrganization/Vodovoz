using System;
using Gtk;
using QS.Dialog.GtkUI;

namespace Vodovoz.MainMenu.ViewMenu
{
	/// <summary>
	/// Обработчик для создания и управления меню Вкладки
	/// </summary>
	public class TabsMenuItemHandler : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private CheckMenuItem _reorderTabsMenuItem;
		private CheckMenuItem _highlightTabsWithColorMenuItem;
		private CheckMenuItem _keepTabColorMenuItem;

		public TabsMenuItemHandler(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var tabsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Вкладки");
			var tabsMenu = new Menu();
			tabsMenuItem.Submenu = tabsMenu;

			_reorderTabsMenuItem = _concreteMenuItemCreator.CreateCheckMenuItem("Перемещение вкладок", OnReorderTabsToggled);
			_highlightTabsWithColorMenuItem =
				_concreteMenuItemCreator.CreateCheckMenuItem("Выделение вкладок цветом", OnHighlightTabsWithColorToggled);
			_keepTabColorMenuItem = _concreteMenuItemCreator.CreateCheckMenuItem("Сохранять цвет вкладки", OnKeepTabColorToggled);
			
			tabsMenu.Add(_reorderTabsMenuItem);
			tabsMenu.Add(_highlightTabsWithColorMenuItem);
			tabsMenu.Add(_keepTabColorMenuItem);
			
			Initialize();

			return tabsMenuItem;
		}

		private void Initialize()
		{
			if(CurrentUserSettings.Settings.ReorderTabs)
			{
				_reorderTabsMenuItem.Activate();
			}

			if(CurrentUserSettings.Settings.HighlightTabsWithColor)
			{
				_highlightTabsWithColorMenuItem.Activate();
			}

			if(CurrentUserSettings.Settings.KeepTabColor)
			{
				_keepTabColorMenuItem.Activate();
			}
		}

		/// <summary>
        /// Перемещение вкладок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReorderTabsToggled(object sender, EventArgs e)
		{
			var isActive = _reorderTabsMenuItem.Active;
			
        	if(CurrentUserSettings.Settings.ReorderTabs != isActive)
        	{
        		CurrentUserSettings.Settings.ReorderTabs = isActive;
        		CurrentUserSettings.SaveSettings();
        		MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
        	}
        }
    
        /// <summary>
        /// Выдление вкладок цветом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHighlightTabsWithColorToggled(object sender, EventArgs e)
        {
        	var isActive = _highlightTabsWithColorMenuItem.Active;
        	
			if(!isActive)
			{
				_keepTabColorMenuItem.Active = false;
			}

			_keepTabColorMenuItem.Sensitive = isActive;
        	
			if(CurrentUserSettings.Settings.HighlightTabsWithColor != isActive)
        	{
        		CurrentUserSettings.Settings.HighlightTabsWithColor = isActive;
        		CurrentUserSettings.SaveSettings();
        		MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
        	}
        }

		/// <summary>
		/// Сохранять цвет вкладки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnKeepTabColorToggled(object sender, EventArgs e)
		{
			var isActive = _keepTabColorMenuItem.Active;
			
			if(CurrentUserSettings.Settings.KeepTabColor != isActive)
			{
				CurrentUserSettings.Settings.KeepTabColor = isActive;
				CurrentUserSettings.SaveSettings();
				MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
			}
		}
	}
}
