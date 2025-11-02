using System;
using System.Collections.Generic;
using Gtk;
using QS.Dialog.GtkUI;

namespace Vodovoz.MainMenu.ViewMenu
{
	public class ThemesAppMenuItemHandler : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly Dictionary<string, string> _themes;
		private bool _themeResetInProcess = false;
		
		public ThemesAppMenuItemHandler(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_themes = new Dictionary<string, string>
			{
				{ "Стандартная", "Breeze" },
				{ "Тёмная", "Mint-Y-Yltra-Dark" }
			};
		}
		
		public MenuItem Create()
		{
			var themesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Темы оформления");
			var themesMenu = new Menu();
			themesMenuItem.Submenu = themesMenu;

			var currentTheme = Gtk.Settings.Default.ThemeName;
			RadioMenuItem lastItem = null;

			foreach(var theme in _themes)
			{
				var themeItem = lastItem is null
					? _concreteMenuItemCreator.CreateRadioMenuItem(theme.Key, ChangeTheme)
					: _concreteMenuItemCreator.CreateRadioMenuItem(theme.Key, ChangeTheme, lastItem);

				lastItem = themeItem;
				themeItem.Active = theme.Value == currentTheme;
				themesMenu.Add(themeItem);
			}

			return themesMenuItem;
		}
		
		private void ChangeTheme(object sender, EventArgs args)
        {
        	if(!(sender is RadioMenuItem menu))
        	{
        		return;
        	}
    
        	if(!menu.Active)
        	{
        		return;
        	}
    
        	if(_themeResetInProcess)
        	{
        		return;
        	}
    
        	if(MessageDialogHelper.RunQuestionDialog("Для применения данной настройки программа будет закрыта.\n Вы уверены?"))
        	{
        		var userGtkRc = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gtkrc-2.0");
    
        		System.IO.File.WriteAllText(userGtkRc, $"gtk-theme-name = \"{_themes[menu.Name]}\"");
    
        		Gtk.Application.Quit();
        	}
        	else
        	{
        		_themeResetInProcess = true;
    
        		string currentTheme = Gtk.Settings.Default.ThemeName;
    
        		foreach(RadioMenuItem menuItem in menu.Group)
        		{
        			menuItem.Active = _themes[menuItem.Name] == currentTheme;
        		}
    
        		_themeResetInProcess = false;
        	}
        }
	}
}
