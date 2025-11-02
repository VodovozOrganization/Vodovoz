using System;
using Gtk;

namespace Vodovoz.MainMenu.HelpMenu
{
	public class HelpMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public HelpMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
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
			Startup.MainWin.NavigationManager.OpenViewModel<Vodovoz.ViewModels.AboutViewModel>(null);
			/*var aboutViewModel = new AboutViewModel(Startup.MainWin.ApplicationInfo);
			var aboutView = new AboutView(aboutViewModel);
			aboutView.ShowAll();
			aboutView.Run();
			aboutView.Destroy();*/
		}
	}
}
