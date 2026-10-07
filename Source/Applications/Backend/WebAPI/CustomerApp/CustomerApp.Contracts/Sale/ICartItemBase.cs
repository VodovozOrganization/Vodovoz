namespace CustomerApp.Contracts.Sale
{
	public interface ICartItemBase
	{
		/// <summary>
		/// Тип товара/услуги
		/// </summary>
		SaleItemType ItemType { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
	}
}
