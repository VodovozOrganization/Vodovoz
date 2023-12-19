using System;
using Gtk;
using Vodovoz.Domain.Employees;
using ToolbarStyle = Gtk.ToolbarStyle;

namespace Vodovoz.MainMenu.ViewMenu
{
	public class MainPanelMenuItemHandler : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private RadioMenuItem _onlyTextToolbarMenuItem;
		private RadioMenuItem _onlyIconsToolbarMenuItem;
		private RadioMenuItem _textAndIconsToolbarMenuItem;
		private RadioMenuItem _extraSmallIconsToolbarMenuItem;
		private RadioMenuItem _smallIconsToolbarMenuItem;
		private RadioMenuItem _middleIconsToolbarMenuItem;
		private RadioMenuItem _largeIconsToolbarMenuItem;

		public MainPanelMenuItemHandler(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var mainPanelMenuItem = _concreteMenuItemCreator.CreateMenuItem("Главная панель");
			var mainPanelMenu = new Menu();
			mainPanelMenuItem.Submenu = mainPanelMenu;

			_onlyTextToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Только текст",
				OnToolBarTextToggled);

			_onlyIconsToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Только иконки",
				OnToolBarIconToggled,
				_onlyTextToolbarMenuItem);

			_textAndIconsToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Текст и иконки",
				OnToolBarBothToggled,
				_onlyIconsToolbarMenuItem);

			_extraSmallIconsToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Очень маленькие иконки",
				OnIconsExtraSmallToggled);

			_smallIconsToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Маленькие иконки",
				OnIconsSmallToggled,
				_extraSmallIconsToolbarMenuItem);

			_middleIconsToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Средние иконки",
				OnIconsMiddleToggled,
				_smallIconsToolbarMenuItem);

			_largeIconsToolbarMenuItem = _concreteMenuItemCreator.CreateRadioMenuItem(
				"Большие иконки",
				OnIconsLargeToggled,
				_middleIconsToolbarMenuItem);

			mainPanelMenu.Add(_onlyTextToolbarMenuItem);
			mainPanelMenu.Add(_onlyIconsToolbarMenuItem);
			mainPanelMenu.Add(_textAndIconsToolbarMenuItem);
			mainPanelMenu.Add(CreateSeparatorMenuItem());
			mainPanelMenu.Add(_extraSmallIconsToolbarMenuItem);
			mainPanelMenu.Add(_smallIconsToolbarMenuItem);
			mainPanelMenu.Add(_middleIconsToolbarMenuItem);
			mainPanelMenu.Add(_largeIconsToolbarMenuItem);
			
			Initialize();

			return mainPanelMenuItem;
		}

		/// <summary>
		/// Только текст
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToolBarTextToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(Domain.Employees.ToolbarStyle.Text);
			}
		}

		/// <summary>
		/// Только иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToolBarIconToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(Domain.Employees.ToolbarStyle.Icons);
			}
		}

		/// <summary>
		/// Иконки и текст
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToolBarBothToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(Domain.Employees.ToolbarStyle.Both);
			}
		}

		/// <summary>
		/// Очень маленькие иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsExtraSmallToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(IconsSize.ExtraSmall);
			}
		}

		/// <summary>
		/// Маленькие иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsSmallToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(IconsSize.Small);
			}
		}

		/// <summary>
		/// Средние иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsMiddleToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(IconsSize.Middle);
			}
		}

		/// <summary>
		/// Большие иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsLargeToggled(object sender, EventArgs e)
		{
			if(sender is RadioMenuItem radioItem && radioItem.Active)
			{
				ToolBarMode(IconsSize.Large);
			}
		}

		private void ToolBarMode(Vodovoz.Domain.Employees.ToolbarStyle style)
		{
			if(CurrentUserSettings.Settings.ToolbarStyle != style)
			{
				CurrentUserSettings.Settings.ToolbarStyle = style;
				CurrentUserSettings.SaveSettings();
			}

			Startup.MainWin.ToolbarMain.ToolbarStyle = (ToolbarStyle)style;
			Startup.MainWin.ToolbarComplaints.ToolbarStyle = (ToolbarStyle)style;

			var result = style != Domain.Employees.ToolbarStyle.Text;
			_extraSmallIconsToolbarMenuItem.Sensitive = result;
			_smallIconsToolbarMenuItem.Sensitive = result;
			_middleIconsToolbarMenuItem.Sensitive = result;
			_largeIconsToolbarMenuItem.Sensitive = result;
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
					Startup.MainWin.ToolbarMain.IconSize = IconSize.SmallToolbar;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.SmallToolbar;
					break;
				case IconsSize.Small:
					Startup.MainWin.ToolbarMain.IconSize = IconSize.LargeToolbar;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.LargeToolbar;
					break;
				case IconsSize.Middle:
					Startup.MainWin.ToolbarMain.IconSize = IconSize.Dnd;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.Dnd;
					break;
				case IconsSize.Large:
					Startup.MainWin.ToolbarMain.IconSize = IconSize.Dialog;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.Dialog;
					break;
			}
		}
		
		private void Initialize()
		{
			ConfigureToolbarStyle();
			ConfigureIconSize();
		}

		private void ConfigureToolbarStyle()
		{
			switch(CurrentUserSettings.Settings.ToolbarStyle)
			{
				case Domain.Employees.ToolbarStyle.Both:
					_textAndIconsToolbarMenuItem.Activate();
					break;
				case Domain.Employees.ToolbarStyle.Icons:
					_onlyIconsToolbarMenuItem.Activate();
					break;
				case Domain.Employees.ToolbarStyle.Text:
					_onlyTextToolbarMenuItem.Activate();
					break;
			}
		}
		
		private void ConfigureIconSize()
		{
			switch(CurrentUserSettings.Settings.ToolBarIconsSize)
			{
				case IconsSize.ExtraSmall:
					_extraSmallIconsToolbarMenuItem.Activate();
					break;
				case IconsSize.Small:
					_smallIconsToolbarMenuItem.Activate();
					break;
				case IconsSize.Middle:
					_middleIconsToolbarMenuItem.Activate();
					break;
				case IconsSize.Large:
					_largeIconsToolbarMenuItem.Activate();
					break;
			}
		}
	}
}
