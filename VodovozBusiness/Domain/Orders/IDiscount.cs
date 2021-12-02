namespace Vodovoz.Domain.Orders
{
	public interface IDiscount
	{
		bool IsDiscountInMoney { get; set; }
		decimal DiscountSetter { get; set; }
		DiscountReason DiscountReason { get; set; }
	}
}
