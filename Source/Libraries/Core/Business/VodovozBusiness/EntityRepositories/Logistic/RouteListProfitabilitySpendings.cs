namespace Vodovoz.EntityRepositories.Logistic
{
	public class RouteListProfitabilitySpendings
	{
		public decimal TotalSales { get; set; }
		public decimal TotalPurchaseSpending { get; set; }
		public decimal TotalInnerDeliverySpending { get; set; }
		public decimal TotalAddressDeliverySpending { get; set; }
		public decimal TotalWarehouseSpending { get; set; }
		public decimal TotalAdministrativeSpending { get; set; }

		public decimal GetTotalSpending()
		{
			return
			TotalPurchaseSpending +
			TotalInnerDeliverySpending +
			TotalAddressDeliverySpending +
			TotalWarehouseSpending +
			TotalAdministrativeSpending;
		}
	}
}
