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

		public int RouteListClosingFinancialIncomeCategoryId => _settingsController.GetValue<int>(nameof(RouteListClosingFinancialIncomeCategoryId).FromPascalCaseToSnakeCase());

		public int RouteListClosingFinancialExpenseCategoryId => _settingsController.GetValue<int>(nameof(RouteListClosingFinancialExpenseCategoryId).FromPascalCaseToSnakeCase());

		public int FuelFinancialExpenseCategoryId => _settingsController.GetValue<int>(nameof(FuelFinancialExpenseCategoryId).FromPascalCaseToSnakeCase());

		/// <summary>
		/// Параметр базы для статьи расхода для авансов.
		/// </summary>
		public int EmployeeSalaryFinancialExpenseCategoryId => _settingsController.GetValue<int>(nameof(EmployeeSalaryFinancialExpenseCategoryId).FromPascalCaseToSnakeCase());
		
		public int DriverReportFinancialIncomeCategoryId => _settingsController.GetValue<int>(nameof(DriverReportFinancialIncomeCategoryId).FromPascalCaseToSnakeCase());

		public int SelfDeliveryDefaultFinancialIncomeCategoryId => _settingsController.GetValue<int>(nameof(SelfDeliveryDefaultFinancialIncomeCategoryId).FromPascalCaseToSnakeCase());

		public int SelfDeliveryDefaultFinancialExpenseCategoryId => _settingsController.GetValue<int>(nameof(SelfDeliveryDefaultFinancialExpenseCategoryId).FromPascalCaseToSnakeCase());
	}
}
