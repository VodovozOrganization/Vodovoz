using System;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.MainMenu.JournalsMenu.Logistics
{
	/// <summary>
	/// Создатель меню Справочники - Логистика - События нахождения водителей на складе
	/// </summary>
	public class DriverWarehouseEventsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public DriverWarehouseEventsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var eventsMenuItem = _concreteMenuItemCreator.CreateMenuItem("События нахождения водителей на складе");
			var eventsMenu = new Menu();
			eventsMenuItem.Submenu = eventsMenu;

			eventsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("События", OnDriversWarehousesEventsActionPressed));
			eventsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Завершенные события", OnCompletedDriversWarehousesEventsPressed));
			
			return eventsMenuItem;
		}
		
		/// <summary>
		/// События
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversWarehousesEventsActionPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DriversWarehousesEventsJournalViewModel>(null);
		}

		/// <summary>
		/// Завершенные события
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCompletedDriversWarehousesEventsPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CompletedDriversWarehousesEventsJournalViewModel>(null);
		}
	}
}
