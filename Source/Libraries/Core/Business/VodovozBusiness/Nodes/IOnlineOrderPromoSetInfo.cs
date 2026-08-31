namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Информация о промонаборе онлайн заказа второй версии
	/// </summary>
	public interface IOnlineOrderPromoSetInfo
	{
		/// <summary>
		/// Наиманование
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Переданная цена
		/// </summary>
		decimal? ReceivedPrice { get; }
		/// <summary>
		/// Переданная сумма
		/// </summary>
		decimal? ReceivedSum { get; }
		/// <summary>
		/// Цена в ДВ
		/// </summary>
		decimal OurPrice { get; }
		/// <summary>
		/// Сумма в ДВ
		/// </summary>
		decimal OurSum { get; }
	}
}
