using System;
using Gtk;
using QS.Project.Journal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;

namespace Vodovoz.MainMenu.ProposalsMenu
{
	/// <summary>
	/// Создатель меню Предложения
	/// </summary>
	public class ProposalsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ProposalsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var proposalsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Предложения");
			var proposalsMenu = new Menu();
			proposalsMenuItem.Submenu = proposalsMenu;

			proposalsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Открыть журнал предложений", OnOpenProposalsJournalPressed));
			
			return proposalsMenuItem;
		}
		
		/// <summary>
		/// Открыть журнал предложений
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOpenProposalsJournalPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager
				.OpenViewModel<ApplicationDevelopmentProposalsJournalViewModel, Action<ApplicationDevelopmentProposalsJournalFilterViewModel>>(
					null,
					filter => filter.HidenByDefault = true,
					QS.Navigation.OpenPageOptions.IgnoreHash,
					vm => vm.SelectionMode = JournalSelectionMode.Multiple);
		}
	}
}
