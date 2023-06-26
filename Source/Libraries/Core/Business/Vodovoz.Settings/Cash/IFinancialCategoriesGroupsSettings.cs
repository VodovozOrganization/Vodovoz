namespace Vodovoz.Settings.Cash
{
	public interface IFinancialCategoriesGroupsSettings
	{
		int DefaultIncomeCategoryId { get; }
		int EmployeeSalaryExpenseCategoryId { get; }
		int FuelExpenseCategoryId { get; }
		int RouteListClosingExpenseCategoryId { get; }
		int RouteListClosingIncomeCategoryId { get; }
		int DriverReportIncomeCategoryId { get; }
		int IncomeSelfDeliveryDefauilCategoryId { get; }
		int ExpenseSelfDeliveryDefauilCategoryId { get; }
	}
}
