using System;
using Gtk;
using QS.Navigation;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.MainMenu.JournalsMenu.Helpers
{
	public class HelpersMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public HelpersMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var helpersMenuItem = _concreteMenuItemCreator.CreateMenuItem("Помощники");
			var helpersMenu = new Menu();
			helpersMenuItem.Submenu = helpersMenu;
		
			helpersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны комментариев", OnCommentTemplatesPressed));
			helpersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны комментариев для штрафов", OnFineCommentTemplatesPressed));
			helpersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны комментариев для премий", OnPremiumCommentTemplatesPressed));

			return helpersMenuItem;
		}
		
		/// <summary>
		/// Шаблоны комментариев
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnCommentTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(CommentTemplate));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Шаблоны комментариев для штрафов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnFineCommentTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(FineTemplate));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Шаблоны комментариев для премий
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPremiumCommentTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<PremiumTemplateJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
