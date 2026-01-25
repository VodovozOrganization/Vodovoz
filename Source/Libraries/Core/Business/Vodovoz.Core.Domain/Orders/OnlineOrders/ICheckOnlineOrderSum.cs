namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	public interface ICheckOnlineOrderSum
	{
		int NomenclatureId { get; }
		decimal Price { get; }
		decimal Count { get; }
		decimal DiscountMoney { get; }
		decimal Sum { get; }
	}
}
