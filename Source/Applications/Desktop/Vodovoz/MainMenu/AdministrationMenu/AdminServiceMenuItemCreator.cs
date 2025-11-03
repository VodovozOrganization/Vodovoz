using System;
using Gtk;
using Vodovoz.ServiceDialogs;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModels.AdministrationTools;

namespace Vodovoz.MainMenu.AdministrationMenu
{
	/// <summary>
	/// Создатель меню Администрирование - Обслуживание
	/// </summary>
	public class AdminServiceMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public AdminServiceMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var adminServiceMenuItem = _concreteMenuItemCreator.CreateMenuItem("Обслуживание");
			var adminServiceMenu = new Menu();
			adminServiceMenuItem.Submenu = adminServiceMenu;
			
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Замена ссылок", OnChangesReferencesPressed));
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Дубликаты адресов", OnAddressDuplicatesPressed));
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Рассчет расстояний до точек", OnDistanceFromCenterPressed));
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Заказы без операций движения бутылей", OnOrdersWithoutBottlesOperationsPressed));
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Переотправка почты", OnResendEmailsPressed));
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Пересчет ЗП водителей", OnRecalculateDriverWagesPressed));
			adminServiceMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Обновление сведений Контрагентов из ФНС", OnUpdateCounterpartyInfoFromFnsPressed));

			return adminServiceMenuItem;
		}
		
		/// <summary>
		/// Замена ссылок
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChangesReferencesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReplaceEntityLinksDlg>(null);
		}

		/// <summary>
		/// Дубликаты адресов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddressDuplicatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<MergeAddressesDlg>(null);
		}

		/// <summary>
		/// Расчет расстояний до точек
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDistanceFromCenterPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<CalculateDistanceToPointsDlg>(null);
		}

		/// <summary>
		/// Заказы без операций движения бутылей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersWithoutBottlesOperationsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<OrdersWithoutBottlesOperationDlg>(null);
		}

		/// <summary>
		/// Переотправка почты
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnResendEmailsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ResendEmailsDialog>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Пересчет ЗП водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRecalculateDriverWagesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<RecalculateDriverWageDlg>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Обновление сведений Контрагентов из ФНС
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void OnUpdateCounterpartyInfoFromFnsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RevenueServiceMassCounterpartyUpdateToolViewModel>(null);
		}
	}
}
