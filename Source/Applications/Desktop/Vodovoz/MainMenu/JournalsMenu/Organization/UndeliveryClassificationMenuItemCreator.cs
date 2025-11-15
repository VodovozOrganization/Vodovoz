using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.MainMenu.JournalsMenu.Organization
{
	/// <summary>
	/// Создатель меню Справочники - Наша организация - Классификация недовозов
	/// </summary>
	public class UndeliveryClassificationMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		
		public UndeliveryClassificationMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var undeliveryClassificationMenuItem = _concreteMenuItemCreator.CreateMenuItem("Классификация недовозов");
			var undeliveryClassificationMenu = new Menu();
			undeliveryClassificationMenuItem.Submenu = undeliveryClassificationMenu;

			undeliveryClassificationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Объекты недовозов", OnUndeliveryObjectsPressed));
			undeliveryClassificationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды недовозов", OnUndeliveryKindsPressed));
			undeliveryClassificationMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Детализация недовозов", OnUndeliveryDetalizationsPressed));

			return undeliveryClassificationMenuItem;
		}
		
		/// <summary>
		/// Объекты недовозов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUndeliveryObjectsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<UndeliveryObjectJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Виды недовозов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUndeliveryKindsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<UndeliveryKindJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Детализация недовозов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUndeliveryDetalizationsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<UndeliveryDetalizationJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
