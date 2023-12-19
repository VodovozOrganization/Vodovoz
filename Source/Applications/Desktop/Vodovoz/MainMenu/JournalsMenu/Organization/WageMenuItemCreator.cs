using System;
using Gtk;
using QS.Navigation;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.MainMenu.JournalsMenu.Organization
{
	public class WageMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public WageMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var wageMenuItem = _concreteMenuItemCreator.CreateMenuItem("Зарплата");
			var wageMenu = new Menu();
			wageMenuItem.Submenu = wageMenu;

			wageMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Зарплатные районы", OnWageDistrictsPressed));
			wageMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Ставки", OnRatesPressed));
			wageMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Планы продаж", OnSalesPlansPressed));
			wageMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды оформления", OnEmployeeRegistrationsPressed));
		
			return wageMenuItem;
		}
		
		/// <summary>
		/// Зарплатные районы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWageDistrictsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<WageDistrictsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Ставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<WageDistrictLevelRatesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Планы продаж
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalesPlansPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<SalesPlanJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Виды оформления
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeeRegistrationsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EmployeeRegistrationsJournalViewModel>(null);
		}
	}
}
