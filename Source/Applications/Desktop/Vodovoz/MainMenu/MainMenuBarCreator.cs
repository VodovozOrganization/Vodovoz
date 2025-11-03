using System;
using Gtk;
using Vodovoz.MainMenu.AdministrationMenu;
using Vodovoz.MainMenu.BaseMenu;
using Vodovoz.MainMenu.HelpMenu;
using Vodovoz.MainMenu.JournalsMenu;
using Vodovoz.MainMenu.ProposalsMenu;
using Vodovoz.MainMenu.ReportsMenu;
using Vodovoz.MainMenu.ViewMenu;

namespace Vodovoz.MainMenu
{
	/// <summary>
	/// Создатель меню главного окна
	/// </summary>
	public class MainMenuBarCreator : MenuBarCreator
	{
		private readonly BaseMenuItemCreator _baseMenuItemCreator;
		private readonly ViewMenuItemCreator _viewMenuItemCreator;
		private readonly JournalsMenuItemCreator _journalsMenuItemCreator;
		private readonly ReportsMenuItemCreator _reportsMenuItemCreator;
		private readonly AdministrationMenuItemCreator _administrationMenuItemCreator;
		private readonly HelpMenuItemCreator _helpMenuItemCreator;
		private readonly ProposalsMenuItemCreator _proposalsMenuItemCreator;

		public MainMenuBarCreator(
			BaseMenuItemCreator baseMenuItemCreator,
			ViewMenuItemCreator viewMenuItemCreator,
			JournalsMenuItemCreator journalsMenuItemCreator,
			ReportsMenuItemCreator reportsMenuItemCreator,
			AdministrationMenuItemCreator administrationMenuItemCreator,
			HelpMenuItemCreator helpMenuItemCreator,
			ProposalsMenuItemCreator proposalsMenuItemCreator)
		{
			_baseMenuItemCreator = baseMenuItemCreator ?? throw new ArgumentNullException(nameof(baseMenuItemCreator));
			_viewMenuItemCreator = viewMenuItemCreator ?? throw new ArgumentNullException(nameof(viewMenuItemCreator));
			_journalsMenuItemCreator = journalsMenuItemCreator ?? throw new ArgumentNullException(nameof(journalsMenuItemCreator));
			_reportsMenuItemCreator = reportsMenuItemCreator ?? throw new ArgumentNullException(nameof(reportsMenuItemCreator));
			_administrationMenuItemCreator =
				administrationMenuItemCreator ?? throw new ArgumentNullException(nameof(administrationMenuItemCreator));
			_helpMenuItemCreator = helpMenuItemCreator ?? throw new ArgumentNullException(nameof(helpMenuItemCreator));
			_proposalsMenuItemCreator = proposalsMenuItemCreator ?? throw new ArgumentNullException(nameof(proposalsMenuItemCreator));
		}
		
		///<inheritdoc/>
		public override MenuBar CreateMenuBar()
		{
			var mainMenuBar = new MenuBar();

			mainMenuBar.Add(_baseMenuItemCreator.Create());
			mainMenuBar.Add(_viewMenuItemCreator.Create());
			mainMenuBar.Add(_journalsMenuItemCreator.Create());
			mainMenuBar.Add(_reportsMenuItemCreator.Create());
			mainMenuBar.Add(_administrationMenuItemCreator.Create());
			mainMenuBar.Add(_helpMenuItemCreator.Create());
			mainMenuBar.Add(_proposalsMenuItemCreator.Create());

			mainMenuBar.ShowAll();
		
			return mainMenuBar;
		}
	}
}
