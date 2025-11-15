using System;
using Gtk;
using Vodovoz.ViewModels;

namespace Vodovoz.MainMenu.HelpMenu
{
	/// <summary>
	/// Создатель меню Справка
	/// </summary>
	public class HelpMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public HelpMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var helpMenuItem = _concreteMenuItemCreator.CreateMenuItem("Справка");
			var helpMenu = new Menu();
			helpMenuItem.Submenu = helpMenu;

			helpMenu.Add(_concreteMenuItemCreator.CreateImageMenuItem(
				"AboutAction",
				"О программе",
				Stock.About,
				null,
				OnAboutPressed));
			
			return helpMenuItem;
		}
		
		/// <summary>
		/// О программе
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAboutPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<AboutViewModel>(null);
		}
	}
}
