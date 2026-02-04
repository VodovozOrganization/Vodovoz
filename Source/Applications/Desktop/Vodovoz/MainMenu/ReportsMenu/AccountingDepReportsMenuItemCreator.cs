using System;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges;
using Vodovoz.ViewModels.Bookkeepping.Reports.EdoControl;
using Vodovoz.ViewModels.ReportsParameters.Bookkeeping;
using Vodovoz.ViewModels.ReportsParameters.Payments;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Бухгалтерия
	/// </summary>
	public class AccountingDepReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public AccountingDepReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		///<inheritdoc/>
		public override MenuItem Create()
		{
			var accountingDepMenuItem = _concreteMenuItemCreator.CreateMenuItem("Бухгалтерия");
			var accountingDepMenu = new Menu();
			accountingDepMenuItem.Submenu = accountingDepMenu;

			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет закрытых отгрузок", OnCloseDeliveryReportPressed));
			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по оплатам (ФО)", OnPaymentsFinDepartmentReportPressed));
			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отсрочка сети", OnChainStoreDelayReportPressed));
			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по изменениям заказа при доставке", OnOrderChangesReportPressed));
			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Долги по безналу", OnCounterpartyCashlessDebtsReportPressed));
			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по УПД в ЧЗ", OnEdoUpdReportPressed));
			accountingDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Контроль за ЭДО", OnEdoControlReportPressed));
			
			return accountingDepMenuItem;
		}
		
		/// <summary>
		/// Отчет закрытых отгрузок
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCloseDeliveryReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CounterpartyCloseDeliveryReportViewModel));
		}

		/// <summary>
		/// Отчет по оплатам (ФО)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaymentsFinDepartmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin
				.NavigationManager
				.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(PaymentsFromBankClientFinDepartmentReportViewModel));
		}

		/// <summary>
		/// Отсрочка сети
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChainStoreDelayReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin
				.NavigationManager
				.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ChainStoreDelayReportViewModel), OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Отчет по изменениям заказа при доставке
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderChangesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OrderChangesReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Долги по безналу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyCashlessDebtsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CounterpartyCashlessDebtsReportViewModel));
		}

		/// <summary>
		/// Отчет по УПД в ЧЗ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEdoUpdReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EdoUpdReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Контроль за ЭДО
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnEdoControlReportPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EdoControlReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
