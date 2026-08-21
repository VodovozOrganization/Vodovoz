using System;
using Gtk;
using QS.Dialog.GtkUI;
using VodovozBusiness.Services.Users;

namespace Vodovoz.MainMenu.ViewMenu
{
	/// <summary>
	/// Обработчик для создания и управления меню Вкладки
	/// </summary>
	public class TabsMenuItemHandler : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly IUserSettingsManager _userSettingsManager;
		private CheckMenuItem _reorderTabsMenuItem;
		private CheckMenuItem _highlightTabsWithColorMenuItem;
		private CheckMenuItem _keepTabColorMenuItem;

		public TabsMenuItemHandler(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			IUserSettingsManager userSettingsManager
			)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_userSettingsManager = userSettingsManager ?? throw new ArgumentNullException(nameof(userSettingsManager));
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
			if(_userSettingsManager.Settings.ReorderTabs)
			{
				_reorderTabsMenuItem.Activate();
			}

			if(_userSettingsManager.Settings.HighlightTabsWithColor)
			{
				_highlightTabsWithColorMenuItem.Activate();
			}

			if(_userSettingsManager.Settings.KeepTabColor)
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
			
        	if(_userSettingsManager.Settings.ReorderTabs != isActive)
        	{
				_userSettingsManager.Settings.ReorderTabs = isActive;
				_userSettingsManager.SaveSettings();
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
        	
			if(_userSettingsManager.Settings.HighlightTabsWithColor != isActive)
        	{
				_userSettingsManager.Settings.HighlightTabsWithColor = isActive;
				_userSettingsManager.SaveSettings();
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
			
			if(_userSettingsManager.Settings.KeepTabColor != isActive)
			{
				_userSettingsManager.Settings.KeepTabColor = isActive;
				_userSettingsManager.SaveSettings();
				MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
			}
		}
	}
}
