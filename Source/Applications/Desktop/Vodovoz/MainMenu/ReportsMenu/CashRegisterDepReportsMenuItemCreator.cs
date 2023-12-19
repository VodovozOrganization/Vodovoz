using System;
using Autofac;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report.ViewModels;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Employees;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.Reports;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ReportsParameters.Cash;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class CashRegisterDepReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public CashRegisterDepReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var cashRegisterDepMenuItem = _concreteMenuItemCreator.CreateMenuItem("Касса");
			var cashRegisterDepMenu = new Menu();
			cashRegisterDepMenuItem.Submenu = cashRegisterDepMenu;

			cashRegisterDepMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("По приходу наличных денежных средств", OnIncomeBalanceReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Зарплаты водителей", OnDriverWagesPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Баланс водителей", OnDriversWageBalancePressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по выдаче бензина", OnFuelReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Зарплаты экспедиторов", OnForwarderWagesReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Зарплаты сотрудников", OnWagesOperationsPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Кассовая книга", OnCashBookReportPressed));
			cashRegisterDepMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Дата ЗП у водителей/экспедиторов", OnDayOfSalaryGiveoutReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по перемещениям с производств",
				OnProductionWarehouseMovementReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Ставки", OnSalaryRatesReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Налоги сотрудников", OnEmployeesTaxesPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Анализ движения денежных средств", OnCashFlowAnalysisPressed));
			cashRegisterDepMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Контроль оплаты перемещений", OnMovementsPaymentControlReportPressed));

			return cashRegisterDepMenuItem;
		}

		/// <summary>
		/// По приходу наличных денежных средств
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIncomeBalanceReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<IncomeBalanceReport>(),
				() => new QSReport.ReportViewDlg(new IncomeBalanceReport()));
		}

		/// <summary>
		/// Зарплаты водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriverWagesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.DriverWagesReport>(),
				() => new QSReport.ReportViewDlg(
					new Vodovoz.Reports.DriverWagesReport(Startup.MainWin.NavigationManager)));
		}

		/// <summary>
		/// Баланс водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversWageBalancePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<DriversWageBalanceReport>(),
				() => new QSReport.ReportViewDlg(new DriversWageBalanceReport()));
		}

		/// <summary>
		/// Отчет по выдаче бензина
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFuelReportPressed(object sender, ButtonPressEventArgs e)
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var report = scope.Resolve<Vodovoz.Reports.FuelReport>();

			var tab = Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.FuelReport>(),
				() => new QSReport.ReportViewDlg(report));

			report.Destroyed += (_, _2) => scope?.Dispose();
		}

		/// <summary>
		/// Зарплаты экспедиторов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnForwarderWagesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.ForwarderWageReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.ForwarderWageReport(Startup.MainWin.NavigationManager)));
		}

		/// <summary>
		/// Зарплаты сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWagesOperationsPressed(object sender, ButtonPressEventArgs e)
		{
			EmployeeFilterViewModel employeeFilter;
			
			

			if(false)
			{
				employeeFilter = new EmployeeFilterViewModel(EmployeeCategory.office);
				employeeFilter.SetAndRefilterAtOnce(
					x => x.Category = EmployeeCategory.driver,
					x => x.Status = EmployeeStatus.IsWorking);
			}
			else
			{
				employeeFilter = new EmployeeFilterViewModel();
				employeeFilter.SetAndRefilterAtOnce(x => x.Status = EmployeeStatus.IsWorking);
			}

			employeeFilter.HidenByDefault = true;
			var employeeJournalFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, employeeFilter);

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.WagesOperationsReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.WagesOperationsReport(employeeJournalFactory)));
		}

		/// <summary>
		/// Кассовая книга
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCashBookReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CashBookReport>(),
				() => new QSReport.ReportViewDlg(new CashBookReport(
					new SubdivisionRepository(new ParametersProvider()), ServicesConfig.CommonServices)));
		}

		/// <summary>
		/// Дата ЗП у водителей/экспедиторов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDayOfSalaryGiveoutReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DayOfSalaryGiveoutReportViewModel));
		}

		/// <summary>
		/// Отчет по контролю оплаты перемещений
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMovementsPaymentControlReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(MovementsPaymentControlViewModel));
		}

		/// <summary>
		/// Отчет по перемещениям с производств
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProductionWarehouseMovementReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ProductionWarehouseMovementReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Ставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalaryRatesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<SalaryRatesReport>(),
				() => new QSReport.ReportViewDlg(new SalaryRatesReport(
					UnitOfWorkFactory.GetDefaultFactory,
					new BaseParametersProvider(new ParametersProvider()),
					ServicesConfig.CommonServices)));
		}

		/// <summary>
		/// Налоги сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeesTaxesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<EmployeesTaxesSumReport>(),
				() => new QSReport.ReportViewDlg(new EmployeesTaxesSumReport(UnitOfWorkFactory.GetDefaultFactory)));
		}

		/// <summary>
		/// Анализ движения денежных средств
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCashFlowAnalysisPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CashFlowAnalysisViewModel>(null);
		}
	}
}
