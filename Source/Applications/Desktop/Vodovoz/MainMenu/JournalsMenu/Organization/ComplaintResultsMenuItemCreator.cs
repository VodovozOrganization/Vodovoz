using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints.ComplaintResults;

namespace Vodovoz.MainMenu.JournalsMenu.Organization
{
	/// <summary>
	/// Создатель меню Справочники - Наша организация - Результаты рассмотрения рекламаций
	/// </summary>
	public class ComplaintResultsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ComplaintResultsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var complaintResultsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Результаты рассмотрения рекламаций");
			var complaintResultsMenu = new Menu();
			complaintResultsMenuItem.Submenu = complaintResultsMenu;

			complaintResultsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Результаты рассмотрения рекламаций по клиенту",
				OnComplaintResultsOfCounterpartyActionActivated));
			
			complaintResultsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Результаты рассмотрения рекламаций по сотрудникам",
				OnComplaintResultsOfEmployeesActionActivated));

			return complaintResultsMenuItem;
		}
		
		/// <summary>
		/// Результаты рассмотрения рекламаций по клиенту
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnComplaintResultsOfCounterpartyActionActivated(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager
				.OpenViewModel<ComplaintResultsOfCounterpartyJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Результаты рассмотрения рекламаций по сотрудникам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnComplaintResultsOfEmployeesActionActivated(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ComplaintResultsOfEmployeesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
