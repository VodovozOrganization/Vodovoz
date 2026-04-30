namespace CustomerOrders.Contracts.Interfaces
{
	public interface IReceivedDiscountData : IDiscountDataBase
	{
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		int? DiscountReasonId { get; set; }
	}
}
