using System;
using System.Collections.Generic;
using Gtk;
using QS.Dialog.GtkUI;

namespace Vodovoz.MainMenu.ViewMenu
{
	/// <summary>
	/// Обработчик для создания и работы с меню Темы оформления
	/// </summary>
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
		
		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var themesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Темы оформления");
			var themesMenu = new Menu();
			themesMenuItem.Submenu = themesMenu;

			var currentTheme = Gtk.Settings.Default.ThemeName;
			RadioAction lastItem = null;

			foreach(var theme in _themes)
			{
				var themeAction = lastItem is null
					? _concreteMenuItemCreator.CreateRadioAction(theme.Value, theme.Key)
					: _concreteMenuItemCreator.CreateRadioAction(theme.Value, theme.Key, actionGroup: lastItem);
				
				lastItem = themeAction;
				themeAction.Active = theme.Value == currentTheme;
				themeAction.Toggled += ChangeTheme;
				themesMenu.Add(themeAction.CreateMenuItem());
			}

			return themesMenuItem;
		}
		
		private void ChangeTheme(object sender, EventArgs args)
        {
        	if(!(sender is RadioAction themeAction))
        	{
        		return;
        	}
    
        	if(!themeAction.Active)
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
    
        		System.IO.File.WriteAllText(userGtkRc, $"gtk-theme-name = \"{_themes[themeAction.Label]}\"");
    
        		Gtk.Application.Quit();
        	}
        	else
        	{
        		_themeResetInProcess = true;
    
        		var currentTheme = Gtk.Settings.Default.ThemeName;
    
        		foreach(RadioMenuItem menuItem in themeAction.Group)
        		{
        			menuItem.Active = _themes[themeAction.Label] == currentTheme;
        		}
    
        		_themeResetInProcess = false;
        	}
        }
	}
}
