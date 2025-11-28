using System;
using Gtk;
using QS.Navigation;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Profitability;

namespace Vodovoz.MainMenu.JournalsMenu.Financies
{
	/// <summary>
	/// Создатель меню Справочники - Финансы
	/// </summary>
	public class FinancesMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly CompanyBalanceMenuItemCreator _companyBalanceMenuItemCreator;
		private MenuItem _profitabilityConstantsMenuItem;

		public FinancesMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			CompanyBalanceMenuItemCreator companyBalanceMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_companyBalanceMenuItemCreator = companyBalanceMenuItemCreator ?? throw new ArgumentNullException(nameof(companyBalanceMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var financesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Финансы");
			var financesMenu = new Menu();
			financesMenuItem.Submenu = financesMenu;

			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Статьи доходов", OnIncomeCategoriesPressed));
			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Статьи расходов", OnExpenseCategoriesPressed));

			_profitabilityConstantsMenuItem =
				_concreteMenuItemCreator.CreateMenuItem("Константы для рентабельности", OnProfitabilityConstantsPressed);
			financesMenu.Add(_profitabilityConstantsMenuItem);
			
			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Финансовые статьи", OnFinancialCategoriesGroupsPressed));
			financesMenu.Add(_companyBalanceMenuItemCreator.Create());
			financesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Категории штрафов", OnFineCategoryJournalPressed));

			Configure();

			return financesMenuItem;
		}

		private void Configure()
		{
			_profitabilityConstantsMenuItem.Sensitive =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_read_and_edit_profitability_constants");
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
		
		/// <summary>
		/// Категории штрафов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFineCategoryJournalPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FineCategoryJournalViewModel>(null);
		}
	}
}
