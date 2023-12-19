using System;
using Autofac;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using Vodovoz.Parameters;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bookkeeping;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class AccountingDepReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public AccountingDepReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
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
			
			return accountingDepMenuItem;
		}
		
		/// <summary>
		/// Отчет закрытых отгрузок
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCloseDeliveryReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CounterpartyCloseDeliveryReport>(),
				() => new QSReport.ReportViewDlg(new CounterpartyCloseDeliveryReport()));
		}

		/// <summary>
		/// Отчет по оплатам (ФО)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaymentsFinDepartmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientFinDepartmentReport>(),
				() => new QSReport.ReportViewDlg(new PaymentsFromBankClientFinDepartmentReport()));
		}

		/// <summary>
		/// Отсрочка сети
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChainStoreDelayReportPressed(object sender, ButtonPressEventArgs e)
		{
			var lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			var report = new ChainStoreDelayReport(
				lifetimeScope,
				lifetimeScope.Resolve<IEmployeeJournalFactory>(),
				lifetimeScope.Resolve<ICounterpartyJournalFactory>(),
				lifetimeScope.Resolve<Vodovoz.Settings.Counterparty.ICounterpartySettings>());
			
			report.Destroyed += (o, args) => lifetimeScope.Dispose();
			
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ChainStoreDelayReport>(),
				() => new QSReport.ReportViewDlg(report));
		}

		/// <summary>
		/// Отчет по изменениям заказа при доставке
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderChangesReportPressed(object sender, ButtonPressEventArgs e)
		{
			var paramProvider = new ParametersProvider();

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrderChangesReport>(),
				() => new QSReport.ReportViewDlg(
					new OrderChangesReport(
						new ReportDefaultsProvider(paramProvider),
						ServicesConfig.InteractiveService,
						new ArchiveDataSettings(paramProvider))));
		}

		/// <summary>
		/// Долги по безналу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyCashlessDebtsReportPressed(object sender, ButtonPressEventArgs e)
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();
			
			var report = new CounterpartyCashlessDebtsReport(
				scope,
				new DeliveryScheduleParametersProvider(new ParametersProvider()),
				ServicesConfig.InteractiveService,
				new CounterpartyJournalFactory(),
				UnitOfWorkFactory.GetDefaultFactory);

			report.Destroyed += (o, args) =>  scope.Dispose();
			
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CounterpartyCashlessDebtsReport>(),
				() => new QSReport.ReportViewDlg(report));
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
	}
}
