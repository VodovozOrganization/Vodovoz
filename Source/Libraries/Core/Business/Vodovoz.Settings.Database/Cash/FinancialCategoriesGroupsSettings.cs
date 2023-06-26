using Vodovoz.Settings.Cash;

namespace Vodovoz.Settings.Database.Cash
{
	public sealed class FinancialCategoriesGroupsSettings : IFinancialCategoriesGroupsSettings
	{
		private readonly ISettingsController _settingsController;

		public FinancialCategoriesGroupsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController
				?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public int DefaultIncomeCategoryId => _settingsController.GetValue<int>("default_income_category_id");

		public int RouteListClosingIncomeCategoryId => _settingsController.GetValue<int>("routelist_income_category_id");

		public int RouteListClosingExpenseCategoryId => _settingsController.GetValue<int>("routelist_expense_category_id");

		public int FuelExpenseCategoryId => _settingsController.GetValue<int>("fuel_expense_categoty_id");

		/// <summary>
		/// Параметр базы для статьи расхода для авансов.
		/// </summary>
		public int EmployeeSalaryExpenseCategoryId => _settingsController.GetValue<int>("employee_salary_expense_category_id");
		
		// TODO: Старая категория #1, не забудь проставить в базе;
		public int DriverReportIncomeCategoryId => _settingsController.GetValue<int>("driver_income_category_id");

		public int IncomeSelfDeliveryDefauilCategoryId => _settingsController.GetValue<int>("income_self_delivery_default_financial_category_id");

		public int ExpenseSelfDeliveryDefauilCategoryId => _settingsController.GetValue<int>("expense_self_delivery_default_financial_category_id");
	}
}
