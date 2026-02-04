namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	/// <summary>
	/// Данные для проверки суммы онлайн заказа
	/// </summary>
	public interface ICheckOnlineOrderSum
	{
		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		int NomenclatureId { get; }
		/// <summary>
		/// Стоимость
		/// </summary>
		decimal Price { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		decimal DiscountMoney { get; }
		/// <summary>
		/// Сумма позиции
		/// </summary>
		decimal Sum { get; }
	}
}
