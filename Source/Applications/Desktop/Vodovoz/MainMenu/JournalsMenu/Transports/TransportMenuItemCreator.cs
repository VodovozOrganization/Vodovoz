using System;
using Gtk;
using QS.Navigation;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.MainMenu.JournalsMenu.Transports
{
	/// <summary>
	/// Создатель меню Справочники - Транспорт
	/// </summary>
	public class TransportMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public TransportMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var transportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Транспорт");
			var transportsMenu = new Menu();
			transportsMenuItem.Submenu = transportsMenu;
			
			transportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(Startup.MainWin.CarsJournalAction));
			transportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды топлива", OnFuelTypesPressed));
			transportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Модели автомобилей", OnCarModelsPressed));
			transportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Производители автомобилей", OnCarManufacturersPressed));
			transportsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды событий ТС", OnCarEventTypesPressed));
			
			return transportsMenuItem;
		}
		
		/// <summary>
		/// Виды топлива
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFuelTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FuelTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Модели автомобилей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarModelsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarModelJournalViewModel>(null);
		}

		/// <summary>
		/// Производители автомобилей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarManufacturersPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarManufacturerJournalViewModel>(null);
		}

		/// <summary>
		/// Виды событий ТС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarEventTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarEventTypeJournalViewModel>(null);
		}
	}
}
