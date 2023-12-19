using System;
using Gtk;
using QS.Navigation;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Profitability;

namespace Vodovoz.MainMenu.JournalsMenu.Financies
{
	public class FinancesMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public FinancesMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var financesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Финансы");
			var financesMenu = new Menu();
			financesMenuItem.Submenu = financesMenu;

			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Статьи доходов", OnIncomeCategoriesPressed));
			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Статьи расходов", OnExpenseCategoriesPressed));
			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Константы для рентабельности", OnProfitabilityConstantsPressed));
			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Финансовые статьи", OnFinancialCategoriesGroupsPressed));

			return financesMenuItem;
		}
		
		/// <summary>
		/// Статьи доходов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIncomeCategoriesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<IncomeCategoryJournalViewModel>(null);
		}

		/// <summary>
		/// Статьи расходов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExpenseCategoriesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ExpenseCategoryJournalViewModel>(null);
		}

		/// <summary>
		/// Константы для рентабельности
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProfitabilityConstantsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ProfitabilityConstantsViewModel, IValidator>(
				null, ServicesConfig.ValidationService, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Финансовые статьи
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFinancialCategoriesGroupsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FinancialCategoriesGroupsJournalViewModel>(null);
		}
	}
}
