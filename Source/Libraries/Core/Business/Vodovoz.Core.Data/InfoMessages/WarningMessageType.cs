namespace Vodovoz.Core.Data.InfoMessages
{
	/// <summary>
	/// Тип предупреждения
	/// </summary>
	public enum WarningMessageType
	{
		/// <summary>
		/// Товар закончился
		/// </summary>
		ItemOutOfStock,
		/// <summary>
		/// Весь товар закончился
		/// </summary>
		AllItemsOutOfStock,
		/// <summary>
		/// Доставка изменилась
		/// </summary>
		DeliveryChanged,
		/// <summary>
		/// Промо набор недоступен
		/// </summary>
		PromoSetInvalid,
		/// <summary>
		/// Промокод недоступен
		/// </summary>
		PromoCodeInvalid,
		/// <summary>
		/// Цена товара изменилась
		/// </summary>
		PriceChanged
	}
}
