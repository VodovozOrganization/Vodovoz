namespace Vodovoz.Domain.Orders
{
	public interface IDiscount
	{
		bool IsDiscountInMoney { get; }
		decimal DiscountSetter { get; }
		DiscountReason DiscountReason { get; }
		void SetDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason);
	}
}
