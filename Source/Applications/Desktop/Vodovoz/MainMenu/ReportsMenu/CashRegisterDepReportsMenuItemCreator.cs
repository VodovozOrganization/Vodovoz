using System;
using Autofac;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QSReport;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Reports;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Employees;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.Services;
using Vodovoz.ViewModels.Cash.Reports;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using Vodovoz.ViewModels.ReportsParameters.Fuel;
using Vodovoz.ViewModels.ReportsParameters.Sales;
using Vodovoz.ViewModels.ReportsParameters.Wages;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class CashRegisterDepReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _incomeBalanceMenuItem;
		private MenuItem _driversWagesMenuItem;
		private MenuItem _driversWageBalanceMenuItem;
		private MenuItem _forwarderWagesMenuItem;
		private MenuItem _employeesTaxesMenuItem;
		private MenuItem _wagesOperationsMenuItem;
		private MenuItem _salaryRatesMenuItem;
		private MenuItem _cashBookMenuItem;
		private MenuItem _cashFlowAnalysisMenuItem;
		private bool _hasAccessToSalariesForLogistics;

		public CashRegisterDepReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var cashRegisterDepMenuItem = _concreteMenuItemCreator.CreateMenuItem("Касса");
			var cashRegisterDepMenu = new Menu();
			cashRegisterDepMenuItem.Submenu = cashRegisterDepMenu;

			_incomeBalanceMenuItem =
				_concreteMenuItemCreator.CreateMenuItem("По приходу наличных денежных средств", OnIncomeBalanceReportPressed);
			cashRegisterDepMenu.Add(_incomeBalanceMenuItem);
			
			_driversWagesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Зарплаты водителей", OnDriverWagesPressed);
			cashRegisterDepMenu.Add(_driversWagesMenuItem);

			_driversWageBalanceMenuItem = _concreteMenuItemCreator.CreateMenuItem("Баланс водителей", OnDriversWageBalancePressed);
			cashRegisterDepMenu.Add(_driversWageBalanceMenuItem);
			
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по выдаче бензина", OnFuelReportPressed));

			_forwarderWagesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Зарплаты экспедиторов", OnForwarderWagesReportPressed);
			cashRegisterDepMenu.Add(_forwarderWagesMenuItem);

			_wagesOperationsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Зарплаты сотрудников", OnWagesOperationsPressed);
			cashRegisterDepMenu.Add(_wagesOperationsMenuItem);

			_cashBookMenuItem = _concreteMenuItemCreator.CreateMenuItem("Кассовая книга", OnCashBookReportPressed);
			cashRegisterDepMenu.Add(_cashBookMenuItem);
			
			cashRegisterDepMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Дата ЗП у водителей/экспедиторов", OnDayOfSalaryGiveoutReportPressed));
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по перемещениям с производств",
				OnProductionWarehouseMovementReportPressed));

			_salaryRatesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Ставки", OnSalaryRatesReportPressed);
			cashRegisterDepMenu.Add(_salaryRatesMenuItem);

			_employeesTaxesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Налоги сотрудников", OnEmployeesTaxesPressed);
			cashRegisterDepMenu.Add(_employeesTaxesMenuItem);

			_cashFlowAnalysisMenuItem =
				_concreteMenuItemCreator.CreateMenuItem("Анализ движения денежных средств", OnCashFlowAnalysisPressed);
			cashRegisterDepMenu.Add(_cashFlowAnalysisMenuItem);
			
			cashRegisterDepMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Контроль оплаты перемещений", OnMovementsPaymentControlReportPressed));
			
			cashRegisterDepMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по запросам Газпром-нефть", OnFuelApiRequestReportPressed));

			Configure();
			
			return cashRegisterDepMenuItem;
		}

		private void Configure()
		{
			var permissionService = ServicesConfig.CommonServices.CurrentPermissionService;
			
			var hasAccessToSalaries = permissionService.ValidatePresetPermission("access_to_salaries");
			_hasAccessToSalariesForLogistics = permissionService.ValidatePresetPermission("access_to_salary_reports_for_logistics");
			var cashier = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
				Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier);

			_incomeBalanceMenuItem.Sensitive = cashier;
			_cashBookMenuItem.Sensitive = cashier;
			_driversWagesMenuItem.Sensitive = hasAccessToSalaries;
			_wagesOperationsMenuItem.Sensitive = hasAccessToSalaries || _hasAccessToSalariesForLogistics;
			_forwarderWagesMenuItem.Sensitive = hasAccessToSalaries;
			_driversWageBalanceMenuItem.Visible = hasAccessToSalaries;
			_employeesTaxesMenuItem.Sensitive = hasAccessToSalaries;
			
			_cashFlowAnalysisMenuItem.Sensitive =
				permissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.CanGenerateCashFlowDdsReport);
		}

		/// <summary>
		/// По приходу наличных денежных средств
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIncomeBalanceReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(IncomeBalanceReportViewModel));
		}

		/// <summary>
		/// Зарплаты водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriverWagesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DriverWagesReportViewModel));
		}

		/// <summary>
		/// Баланс водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversWageBalancePressed(object sender, ButtonPressEventArgs e)
		{
			var reportInfoFactory = Startup.AppDIContainer.Resolve<IReportInfoFactory>();
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<DriversWageBalanceReport>(),
				() => new QSReport.ReportViewDlg(new DriversWageBalanceReport(reportInfoFactory)));
		}

		/// <summary>
		/// Отчет по выдаче бензина
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFuelReportPressed(object sender, ButtonPressEventArgs e)
		{
			var dlg = Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
					null,
					options: OpenPageOptions.IgnoreHash,
					addingRegistrations: builder => builder.RegisterType<FuelReport>().As<IParametersWidget>())
				.TdiTab;

			var report = (dlg as ReportViewDlg).ParametersWidget;
			(report as FuelReport).ParentTab = dlg;
		}

		/// <summary>
		/// Зарплаты экспедиторов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnForwarderWagesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ForwarderWageReportViewModel));
		}

		/// <summary>
		/// Зарплаты сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWagesOperationsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(WagesOperationsReportViewModel));
		}

		/// <summary>
		/// Кассовая книга
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCashBookReportPressed(object sender, ButtonPressEventArgs e)
		{
			var reportInfoFactory = Startup.AppDIContainer.Resolve<IReportInfoFactory>();
			var commonServices = Startup.AppDIContainer.Resolve<ICommonServices>();
			var subdivisionRepository = Startup.AppDIContainer.Resolve<ISubdivisionRepository>();
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CashBookReport>(),
				() => new QSReport.ReportViewDlg(new CashBookReport(reportInfoFactory, subdivisionRepository, commonServices)));
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
			var uowFactory = Startup.AppDIContainer.Resolve<IUnitOfWorkFactory>();
			var wageSettings = Startup.AppDIContainer.Resolve<IWageSettings>();
			var reportInfoFactory = Startup.AppDIContainer.Resolve<IReportInfoFactory>();
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<SalaryRatesReport>(),
				() => new QSReport.ReportViewDlg(new SalaryRatesReport(
					reportInfoFactory,
					uowFactory,
					wageSettings,
					ServicesConfig.CommonServices)));
		}

		/// <summary>
		/// Налоги сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeesTaxesPressed(object sender, ButtonPressEventArgs e)
		{
			var reportInfoFactory = Startup.AppDIContainer.Resolve<IReportInfoFactory>();
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<EmployeesTaxesSumReport>(),
				() => new QSReport.ReportViewDlg(new EmployeesTaxesSumReport(reportInfoFactory, ServicesConfig.UnitOfWorkFactory)));
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
		
		/// <summary>
		/// Отчет по запросам к API Газпром-нефть
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFuelApiRequestReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FuelApiRequestReportViewModel));
		}
	}
}
