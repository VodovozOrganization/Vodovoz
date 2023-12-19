using System;
using Gtk;
using QS.Banks.Domain;
using QS.Navigation;
using QSBanks;
using QSOrmProject;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.MainMenu.JournalsMenu.Banks
{
	public class BanksMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public BanksMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}
		
		public MenuItem Create()
		{
			var banksMenuItem = _concreteMenuItemCreator.CreateMenuItem("Банки/Операторы ЭДО");
			var banksMenu = new Menu();
			banksMenuItem.Submenu = banksMenu;

			banksMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Банки РФ", OnBanksRfPressed));
			banksMenu.Add(CreateSeparatorMenuItem());
			banksMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Обновить с сайта Центрального банка", OnUpdateBanksFromCbrPressed));
			banksMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Операторы ЭДО", OnEdoOperatorsPressed));

			return banksMenuItem;
		}
		
		/// <summary>
		/// Банки РФ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnBanksRfPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(Bank));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Обновить с сайта Центрального банка
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpdateBanksFromCbrPressed(object sender, ButtonPressEventArgs e)
		{
			BanksUpdater.CheckBanksUpdate(true);
		}

		/// <summary>
		/// Операторы ЭДО
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEdoOperatorsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EdoOperatorsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
