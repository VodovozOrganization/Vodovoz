namespace Vodovoz.Domain.Orders
{
	public interface IDiscount
	{
		bool IsDiscountInMoney { get; }
		DiscountReason DiscountReason { get; }
		void SetDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason);
	}
}
