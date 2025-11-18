using System;
using Gtk;
using QS.Project.Services;
using QS.Report.ViewModels;
using Vodovoz.ViewModels.ReportsParameters.Wages;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Сотрудники
	/// </summary>
	public class EmployeesReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _employeesBonuses;
		private MenuItem _employeesfines;

		public EmployeesReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		///<inheritdoc/>
		public override MenuItem Create()
		{
			var employeesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Сотрудники");
			var employeesMenu = new Menu();
			employeesMenuItem.Submenu = employeesMenu;

			_employeesfines = _concreteMenuItemCreator.CreateMenuItem("Штрафы сотрудников", OnEmployeeFinesReportPressed);
			employeesMenu.Add(_employeesfines);
			_employeesBonuses = _concreteMenuItemCreator.CreateMenuItem("Премии сотрудников", OnEmployeesBonusesReportPressed);
			employeesMenu.Add(_employeesBonuses);

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
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(EmployeesFinesViewModel));
		}

		/// <summary>
		/// Премии сотрудников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeesBonusesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(EmployeesPremiumsViewModel));
		}
	}
}
