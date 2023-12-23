using System;
using Gtk;
using QS.Project.Services;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Employees;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class EmployeesReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _employeesBonuses;
		private MenuItem _employeesfines;

		public EmployeesReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var employeesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Сотрудники");
			var employeesMenu = new Menu();
			employeesMenuItem.Submenu = employeesMenu;

			_employeesfines = _concreteMenuItemCreator.CreateMenuItem("Штрафы сотрудников", OnEmployeeFinesReportPressed);
			employeesMenu.Add(_employeesfines);

			_employeesBonuses = _concreteMenuItemCreator.CreateMenuItem("Премии сотрудников", OnEmployeesBonusesReportPressed);
			employeesMenu.Add(_employeesBonuses);
			
			employeesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по сотрудникам", OnEmployeesReportPressed));

			Configure();
			
			return employeesMenuItem;
		}

		private void Configure()
		{
			var permissionService = ServicesConfig.CommonServices.CurrentPermissionService;
			var hasAccessToWagesAndBonuses = permissionService.ValidatePresetPermission("access_to_fines_bonuses");
			
			_employeesBonuses.Sensitive = hasAccessToWagesAndBonuses;
			_employeesfines.Sensitive = hasAccessToWagesAndBonuses;
		}
		
		/// <summary>
		/// Штрафы сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeeFinesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EmployeesFines>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EmployeesFines()));
		}

		/// <summary>
		/// Премии сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeesBonusesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<EmployeesPremiums>(),
				() => new QSReport.ReportViewDlg(new EmployeesPremiums()));
		}

		/// <summary>
		/// Отчет по сотрудникам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<EmployeesReport>(),
				() => new QSReport.ReportViewDlg(new EmployeesReport(ServicesConfig.InteractiveService)));
		}
	}
}
