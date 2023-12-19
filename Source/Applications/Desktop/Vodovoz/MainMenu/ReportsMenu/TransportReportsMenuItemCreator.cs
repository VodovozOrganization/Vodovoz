using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.ViewModels.Reports;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class TransportReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public TransportReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var transportMenuItem = _concreteMenuItemCreator.CreateMenuItem("Транспорт");
			var transportMenu = new Menu();
			transportMenuItem.Submenu = transportMenu;

			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Затраты при эксплуатации ТС", OnCostCarExploitationReportPressed));
			
			return transportMenuItem;
		}
		
		/// <summary>
		/// Затраты при эксплуатации ТС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCostCarExploitationReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CostCarExploitationReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
