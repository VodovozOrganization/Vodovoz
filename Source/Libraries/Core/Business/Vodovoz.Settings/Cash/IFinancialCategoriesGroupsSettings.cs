namespace Vodovoz.Settings.Cash
{
	public interface IFinancialCategoriesGroupsSettings
	{
		int EmployeeSalaryFinancialExpenseCategoryId { get; }
		int FuelFinancialExpenseCategoryId { get; }
		int RouteListClosingFinancialExpenseCategoryId { get; }
		int RouteListClosingFinancialIncomeCategoryId { get; }
		int DriverReportFinancialIncomeCategoryId { get; }
		int SelfDeliveryDefaultFinancialIncomeCategoryId { get; }
		int SelfDeliveryDefaultFinancialExpenseCategoryId { get; }
	}
}
