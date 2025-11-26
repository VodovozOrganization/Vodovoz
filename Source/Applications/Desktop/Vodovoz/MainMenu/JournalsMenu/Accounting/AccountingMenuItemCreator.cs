using System;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.MainMenu.JournalsMenu.Accounting
{
	/// <summary>
	/// Создатель меню Справочники - Бухгалтерия
	/// </summary>
	public class AccountingMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public AccountingMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var accountingMenuItem = _concreteMenuItemCreator.CreateMenuItem("Бухгалтерия");
			var accountingMenu = new Menu();
			accountingMenuItem.Submenu = accountingMenu;
			
			accountingMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Контрагенты, которые не участвуют в распределении", OnNotAllocatedCounterpartiesJournalPressed));
			accountingMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Категории дохода", OnProfitCategoriesJournalPressed));
			
			return accountingMenuItem;
		}
		
		/// <summary>
		/// Справочник контрагентов, которые не участвуют в распределени
		/// </summary>
		/// <param name="sender">Инициатор</param>
		/// <param name="e">Аргументы</param>
		private void OnNotAllocatedCounterpartiesJournalPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<NotAllocatedCounterpartiesJournalViewModel>(null);
		}

		/// <summary>
		/// Справочник категорий дохода
		/// </summary>
		/// <param name="sender">Инициатор</param>
		/// <param name="e">Аргументы</param>
		private void OnProfitCategoriesJournalPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ProfitCategoriesJournalViewModel>(null);
		}
	}
}
