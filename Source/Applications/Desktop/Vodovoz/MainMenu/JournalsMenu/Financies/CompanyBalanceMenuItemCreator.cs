using System;
using Gtk;
using QS.Project.Domain;
using Vodovoz.Presentation.ViewModels.Organisations;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.MainMenu.JournalsMenu.Financies
{
	public class CompanyBalanceMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public CompanyBalanceMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var companyBalanceMenuItem = _concreteMenuItemCreator.CreateMenuItem("Остатки ден.средств по компании");
			var companyBalanceMenu = new Menu();
			companyBalanceMenuItem.Submenu = companyBalanceMenu;
			
			companyBalanceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Остатки ден.средств по компании на дату", OnCompanyBalanceByDatePressed));
			companyBalanceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Направления бизнеса", OnBusinessActivitiesPressed));
			companyBalanceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Расчетные счета", OnBusinessAccountsPressed));
			companyBalanceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Формы ден.средств", OnFundsPressed));
			
			return companyBalanceMenuItem;
		}
		
		/// <summary>
		/// Остатки ден.средств по компании на дату
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCompanyBalanceByDatePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CompanyBalanceByDateViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());
		}

		/// <summary>
		/// Направления бизнеса
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBusinessActivitiesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<BusinessActivitiesJournalViewModel>(null);
		}

		/// <summary>
		/// Расчетные счета
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBusinessAccountsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<BusinessAccountsJournalViewModel>(null);
		}

		/// <summary>
		/// Формы ден.средств
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFundsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FundsJournalViewModel>(null);
		}
	}
}
