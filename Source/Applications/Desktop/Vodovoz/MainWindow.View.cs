using Gtk;
using MoreLinq;
using QS.Dialog.GtkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz;
using Vodovoz.Core.Domain.Users.Settings;
using ToolbarStyle = Vodovoz.Core.Domain.Users.Settings.ToolbarStyle;

public partial class MainWindow
{
	private Dictionary<string, string> _themes;
	private bool _themeResetInProcess = false;
	/*
	#region Главная панель

	/// <summary>
	/// Только текст
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionToolBarTextToggled(object sender, EventArgs e)
	{
		if(ActionToolBarText.Active)
		{
			ToolBarMode(ToolbarStyle.Text);
		}
	}

	/// <summary>
	/// Только иконки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionToolBarIconToggled(object sender, EventArgs e)
	{
		if(ActionToolBarIcon.Active)
		{
			ToolBarMode(ToolbarStyle.Icons);
		}
	}

	/// <summary>
	/// Иконки и текст
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionToolBarBothToggled(object sender, EventArgs e)
	{
		if(ActionToolBarBoth.Active)
		{
			ToolBarMode(ToolbarStyle.Both);
		}
	}

	/// <summary>
	/// Очень маленькие иконки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsExtraSmall.Active)
		{
			ToolBarMode(IconsSize.ExtraSmall);
		}
	}

	/// <summary>
	/// Маленькие иконки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionIconsSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsSmall.Active)
		{
			ToolBarMode(IconsSize.Small);
		}
	}

	/// <summary>
	/// Средние иконки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionIconsMiddleToggled(object sender, EventArgs e)
	{
		if(ActionIconsMiddle.Active)
		{
			ToolBarMode(IconsSize.Middle);
		}
	}

	/// <summary>
	/// Большие иконки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionIconsLargeToggled(object sender, EventArgs e)
	{
		if(ActionIconsLarge.Active)
		{
			ToolBarMode(IconsSize.Large);
		}
	}

	private void ToolBarMode(ToolbarStyle style)
	{
		if(CurrentUserSettings.Settings.ToolbarStyle != style)
		{
			CurrentUserSettings.Settings.ToolbarStyle = style;
			CurrentUserSettings.SaveSettings();
		}

		toolbarMain.ToolbarStyle = (Gtk.ToolbarStyle)style;
		tlbComplaints.ToolbarStyle = (Gtk.ToolbarStyle)style;
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			style != ToolbarStyle.Text;
	}

	private void ToolBarMode(IconsSize size)
	{
		if(CurrentUserSettings.Settings.ToolBarIconsSize != size)
		{
			CurrentUserSettings.Settings.ToolBarIconsSize = size;
			CurrentUserSettings.SaveSettings();
		}

		switch(size)
		{
			case IconsSize.ExtraSmall:
				toolbarMain.IconSize = IconSize.SmallToolbar;
				tlbComplaints.IconSize = IconSize.SmallToolbar;
				pacspanelview1.IconSize = IconSize.SmallToolbar;
				break;
			case IconsSize.Small:
				toolbarMain.IconSize = IconSize.LargeToolbar;
				tlbComplaints.IconSize = IconSize.LargeToolbar;
				pacspanelview1.IconSize = IconSize.LargeToolbar;
				break;
			case IconsSize.Middle:
				toolbarMain.IconSize = IconSize.Dnd;
				tlbComplaints.IconSize = IconSize.Dnd;
				pacspanelview1.IconSize = IconSize.Dnd;
				break;
			case IconsSize.Large:
				toolbarMain.IconSize = IconSize.Dialog;
				tlbComplaints.IconSize = IconSize.Dialog;
				pacspanelview1.IconSize = IconSize.Dialog;
				break;
		}
	}

	#endregion Главная панель

	#region Вкладки

	/// <summary>
	/// Перемещение вкладок
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnReorderTabsToggled(object sender, EventArgs e)
	{
		var isActive = ReorderTabs.Active;

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
	protected void OnHighlightTabsWithColorToggled(object sender, EventArgs e)
	{
		var isActive = HighlightTabsWithColor.Active;
		if(!isActive)
			KeepTabColor.Active = false;
		KeepTabColor.Sensitive = isActive;
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
	protected void OnKeepTabColorToggled(object sender, EventArgs e)
	{
		var isActive = KeepTabColor.Active;
		if(CurrentUserSettings.Settings.KeepTabColor != isActive)
		{
			CurrentUserSettings.Settings.KeepTabColor = isActive;
			CurrentUserSettings.SaveSettings();
			MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
		}
	}

	private string[] GetTabsColors() =>
		new[] { "#F81919", "#009F6B", "#1F8BFF", "#FF9F00", "#FA7A7A", "#B46034", "#99B6FF", "#8F2BE1", "#00CC44" };

	#endregion Вкладки

	#region Темы оформления

	public void InitializeThemesMenuItem()
	{
		_themes = new Dictionary<string, string>();

		string currentTheme = Gtk.Settings.Default.ThemeName;

		string themesPath = Rc.ThemeDir;

		_themes.Add("Стандартная", "Breeze");
		_themes.Add("Тёмная", "Mint-Y-Yltra-Dark");

		var viewMenuItem = menubarMain.Children.Where(x => x.Name == nameof(Action18)).Cast<ImageMenuItem>().FirstOrDefault();

		var submenu = viewMenuItem.Submenu as Menu;

		var themesSubmenuRoot = new MenuItem("Тема оформления");

		var subsubmenu = new Menu();

		themesSubmenuRoot.Submenu = subsubmenu;

		RadioMenuItem lastitem = null;

		foreach(var theme in _themes)
		{
			var themeSubmenu = lastitem is null ? new RadioMenuItem(theme.Key) : new RadioMenuItem(lastitem, theme.Key);

			lastitem = themeSubmenu;

			themeSubmenu.Name = theme.Key;

			themeSubmenu.Active = theme.Value == currentTheme;

			themeSubmenu.Toggled += ChangeTheme;

			subsubmenu.Append(themeSubmenu);
		}

		submenu.Append(themesSubmenuRoot);

		themesSubmenuRoot.ShowAll();
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

	#endregion Темы оформления
	*/
}
