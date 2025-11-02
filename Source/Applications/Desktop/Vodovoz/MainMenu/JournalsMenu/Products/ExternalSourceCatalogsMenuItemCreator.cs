using System;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.MainMenu.JournalsMenu.Products
{
	public class ExternalSourceCatalogsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ExternalSourceCatalogsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var externalSourceCatalogsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Каталоги в ИПЗ");
			var externalSourceCatalogsMenu = new Menu();
			externalSourceCatalogsMenuItem.Submenu = externalSourceCatalogsMenu;
			
			externalSourceCatalogsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Онлайн каталоги сайта ВВ", OnVodovozWebSiteNomenclatureOnlineCatalogsPressed));
			externalSourceCatalogsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Онлайн каталоги мобильного приложения", OnMobileAppNomenclatureOnlineCatalogsPressed));
			externalSourceCatalogsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Онлайн каталоги сайта Кулер Сэйл", OnKulerSaleWebSiteNomenclatureOnlineCatalogsPressed));

			return externalSourceCatalogsMenuItem;
		}
		
		/// <summary>
		/// ИПЗ - Онлайн каталоги - Онлайн каталоги сайта ВВ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnVodovozWebSiteNomenclatureOnlineCatalogsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<VodovozWebSiteNomenclatureOnlineCatalogsJournalViewModel>(null);
		}

		/// <summary>
		/// ИПЗ - Онлайн каталоги - Онлайн каталоги мобильного приложения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMobileAppNomenclatureOnlineCatalogsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<MobileAppNomenclatureOnlineCatalogsJournalViewModel>(null);
		}

		/// <summary>
		/// ИПЗ - Онлайн каталоги - Онлайн каталоги сайта Кулер Сэйл
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnKulerSaleWebSiteNomenclatureOnlineCatalogsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<KulerSaleWebSiteNomenclatureOnlineCatalogsJournalViewModel>(null);
		}
	}
}
