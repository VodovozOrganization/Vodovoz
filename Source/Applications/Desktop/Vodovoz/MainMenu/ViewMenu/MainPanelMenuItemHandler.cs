using System;
using Gtk;
using Vodovoz.Core.Domain.Users.Settings;
using ToolbarStyle = Gtk.ToolbarStyle;

namespace Vodovoz.MainMenu.ViewMenu
{
	/// <summary>
	/// Обработчик для создания и управления меню Главная панель
	/// </summary>
	public class MainPanelMenuItemHandler : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private RadioAction _onlyTextToolbarAction;
		private RadioAction _onlyIconsToolbarAction;
		private RadioAction _textAndIconsToolbarAction;
		private RadioAction _extraSmallIconsToolbarAction;
		private RadioAction _smallIconsToolbarAction;
		private RadioAction _middleIconsToolbarAction;
		private RadioAction _largeIconsToolbarAction;

		public MainPanelMenuItemHandler(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var mainPanelMenuItem = _concreteMenuItemCreator.CreateMenuItem("Главная панель");
			var mainPanelMenu = new Menu();
			mainPanelMenuItem.Submenu = mainPanelMenu;

			_onlyTextToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"OnlyTextAction",
				"Только текст",
				eventHandler: OnToolBarTextActivated);

			_onlyIconsToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"OnlyIconsAction",
				"Только иконки",
				eventHandler: OnToolBarIconActivated,
				actionGroup: _onlyTextToolbarAction);

			_textAndIconsToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"TextAndIconsAction",
				"Текст и иконки",
				eventHandler: OnToolBarBothActivated,
				actionGroup: _onlyIconsToolbarAction);

			_extraSmallIconsToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"ExtraSmallIconsAction",
				"Очень маленькие иконки",
				eventHandler: OnIconsExtraSmallActivated);

			_smallIconsToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"SmallIconsAction",
				"Маленькие иконки",
				eventHandler: OnIconsSmallActivated,
				actionGroup: _extraSmallIconsToolbarAction);

			_middleIconsToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"MiddleIconsAction",
				"Средние иконки",
				eventHandler: OnIconsMiddleActivated,
				actionGroup: _smallIconsToolbarAction);

			_largeIconsToolbarAction = _concreteMenuItemCreator.CreateRadioAction(
				"LargeIconsAction",
				"Большие иконки",
				eventHandler: OnIconsLargeActivated,
				actionGroup: _middleIconsToolbarAction);

			mainPanelMenu.Add(_onlyTextToolbarAction.CreateMenuItem());
			mainPanelMenu.Add(_onlyIconsToolbarAction.CreateMenuItem());
			mainPanelMenu.Add(_textAndIconsToolbarAction.CreateMenuItem());
			mainPanelMenu.Add(CreateSeparatorMenuItem());
			mainPanelMenu.Add(_extraSmallIconsToolbarAction.CreateMenuItem());
			mainPanelMenu.Add(_smallIconsToolbarAction.CreateMenuItem());
			mainPanelMenu.Add(_middleIconsToolbarAction.CreateMenuItem());
			mainPanelMenu.Add(_largeIconsToolbarAction.CreateMenuItem());
			
			return mainPanelMenuItem;
		}

		/// <summary>
		/// Только текст
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToolBarTextActivated(object sender, EventArgs e)
		{
			if(_onlyTextToolbarAction.Active)
			{
				ToolBarMode(Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Text);
			}
		}

		/// <summary>
		/// Только иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToolBarIconActivated(object sender, EventArgs e)
		{
			if(_onlyIconsToolbarAction.Active)
			{
				ToolBarMode(Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Icons);
			}
		}

		/// <summary>
		/// Иконки и текст
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToolBarBothActivated(object sender, EventArgs e)
		{
			if(_textAndIconsToolbarAction.Active)
			{
				ToolBarMode(Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Both);
			}
		}

		/// <summary>
		/// Очень маленькие иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsExtraSmallActivated(object sender, EventArgs e)
		{
			if(_extraSmallIconsToolbarAction.Active)
			{
				ToolBarMode(IconsSize.ExtraSmall);
			}
		}

		/// <summary>
		/// Маленькие иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsSmallActivated(object sender, EventArgs e)
		{
			if(_smallIconsToolbarAction.Active)
			{
				ToolBarMode(IconsSize.Small);
			}
		}

		/// <summary>
		/// Средние иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsMiddleActivated(object sender, EventArgs e)
		{
			if(_middleIconsToolbarAction.Active)
			{
				ToolBarMode(IconsSize.Middle);
			}
		}

		/// <summary>
		/// Большие иконки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIconsLargeActivated(object sender, EventArgs e)
		{
			if(_largeIconsToolbarAction.Active)
			{
				ToolBarMode(IconsSize.Large);
			}
		}

		private void ToolBarMode(Vodovoz.Core.Domain.Users.Settings.ToolbarStyle style)
		{
			if(CurrentUserSettings.Settings.ToolbarStyle != style)
			{
				CurrentUserSettings.Settings.ToolbarStyle = style;
				CurrentUserSettings.SaveSettings();
			}

			Startup.MainWin.ToolbarMain.ToolbarStyle = (ToolbarStyle)style;
			Startup.MainWin.ToolbarComplaints.ToolbarStyle = (ToolbarStyle)style;

			var result = style != Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Text;
			_extraSmallIconsToolbarAction.Sensitive = result;
			_smallIconsToolbarAction.Sensitive = result;
			_middleIconsToolbarAction.Sensitive = result;
			_largeIconsToolbarAction.Sensitive = result;
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
					Startup.MainWin.PacsPanelView.IconSize = IconSize.SmallToolbar;
					break;
				case IconsSize.Small:
					Startup.MainWin.ToolbarMain.IconSize = IconSize.LargeToolbar;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.LargeToolbar;
					Startup.MainWin.PacsPanelView.IconSize = IconSize.LargeToolbar;
					break;
				case IconsSize.Middle:
					Startup.MainWin.ToolbarMain.IconSize = IconSize.Dnd;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.Dnd;
					Startup.MainWin.PacsPanelView.IconSize = IconSize.Dnd;
					break;
				case IconsSize.Large:
					Startup.MainWin.ToolbarMain.IconSize = IconSize.Dialog;
					Startup.MainWin.ToolbarComplaints.IconSize = IconSize.Dialog;
					Startup.MainWin.PacsPanelView.IconSize = IconSize.Dialog;
					break;
			}
		}
		
		public void Initialize()
		{
			ConfigureIconSize();
			ConfigureToolbarStyle();
		}

		private void ConfigureToolbarStyle()
		{
			switch(CurrentUserSettings.Settings.ToolbarStyle)
			{
				case Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Both:
					_textAndIconsToolbarAction.Activate();
					break;
				case Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Icons:
					_onlyIconsToolbarAction.Activate();
					break;
				case Vodovoz.Core.Domain.Users.Settings.ToolbarStyle.Text:
					_onlyTextToolbarAction.Activate();
					break;
			}
		}
		
		private void ConfigureIconSize()
		{
			switch(CurrentUserSettings.Settings.ToolBarIconsSize)
			{
				case IconsSize.ExtraSmall:
					_extraSmallIconsToolbarAction.Activate();
					break;
				case IconsSize.Small:
					_smallIconsToolbarAction.Activate();
					break;
				case IconsSize.Middle:
					_middleIconsToolbarAction.Activate();
					break;
				case IconsSize.Large:
					_largeIconsToolbarAction.Activate();
					break;
			}
		}
	}
}
