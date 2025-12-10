using Gtk;
using QS.Navigation;
using System;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.WageCalculation;

namespace Vodovoz.MainMenu.JournalsMenu.Organization
{
	/// <summary>
	/// Создатель меню Справочники - Наша организация - Зарплата
	/// </summary>
	public class WageMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _wageDistricts;
		private MenuItem _rates;
		private MenuItem _salesPlans;
		private MenuItem _callCenterMotivationCoefficient;

		public WageMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var wageMenuItem = _concreteMenuItemCreator.CreateMenuItem("Зарплата");
			var wageMenu = new Menu();
			wageMenuItem.Submenu = wageMenu;

			_wageDistricts = _concreteMenuItemCreator.CreateMenuItem("Зарплатные районы", OnWageDistrictsPressed);
			wageMenu.Add(_wageDistricts);

			_rates = _concreteMenuItemCreator.CreateMenuItem("Ставки", OnRatesPressed);
			wageMenu.Add(_rates);

			_salesPlans = _concreteMenuItemCreator.CreateMenuItem("Планы продаж", OnSalesPlansPressed);
			wageMenu.Add(_salesPlans);
			
			wageMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды оформления", OnEmployeeRegistrationsPressed));

			_callCenterMotivationCoefficient = _concreteMenuItemCreator.CreateMenuItem("Коэф. для мотивации КЦ", OnCallCenterMotivationCoefficientPressed);
			wageMenu.Add(_callCenterMotivationCoefficient);
			
			Configure();

			return wageMenuItem;
		}
		

		private void Configure()
		{
			var canEditWage = Startup.MainWin.CurrentPermissionService.ValidatePresetPermission("can_edit_wage");
			_wageDistricts.Sensitive = canEditWage;
			_rates.Sensitive = canEditWage;

			var canEditWageBySelfSubdivision = Startup.MainWin.CurrentPermissionService.ValidatePresetPermission("can_edit_wage_by_self_subdivision");
			_salesPlans.Sensitive = canEditWageBySelfSubdivision;
			
			var canEditCallCenterMotivationCoefficient = Startup.MainWin.CurrentPermissionService.ValidatePresetPermission(WageCalculationPermissions.CanEditCallCenterMotivationCoefficient);
			_callCenterMotivationCoefficient.Sensitive = canEditCallCenterMotivationCoefficient;
		}

		/// <summary>
		/// Зарплатные районы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWageDistrictsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<WageDistrictsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Ставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<WageDistrictLevelRatesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Планы продаж
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalesPlansPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<SalesPlanJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Виды оформления
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeeRegistrationsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EmployeeRegistrationsJournalViewModel>(null);
		}
		
		/// <summary>
		/// Коэффициенты для мотивации КЦ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCallCenterMotivationCoefficientPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<CallCenterMotivationCoefficientJournalViewModel>(null);
		}
	}
}
