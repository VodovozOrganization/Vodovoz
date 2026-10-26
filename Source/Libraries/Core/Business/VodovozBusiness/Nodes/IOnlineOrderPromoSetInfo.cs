using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Информация о промонаборе онлайн заказа второй версии
	/// </summary>
	public interface IOnlineOrderPromoSetInfo
	{
		/// <summary>
		/// Наименование
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		
		/// <summary>
		/// Информация о количестве промонаборов для товара("x Count")
		/// </summary>
		string PromoSetsCountMultiplierInfo { get; }

		/// <summary>
		/// Переданная цена
		/// </summary>
		decimal? ReceivedPrice { get; }

		/// <summary>
		/// Переданная сумма
		/// </summary>
		decimal? ReceivedSum { get; }

		/// <summary>
		/// Переданная скидка
		/// </summary>
		decimal? ReceivedDiscountMoney { get; }

		/// <summary>
		/// Цена в ДВ(округленная)
		/// </summary>
		decimal RoundedOurPrice { get; }
		
		/// <summary>
		/// Цена товара из промонабора по прайсу
		/// </summary>
		decimal? NomenclaturePrice { get; }
		
		/// <summary>
		/// Скидка из промонабора
		/// </summary>
		decimal? OurDiscountMoneyFromPromoSet { get; }

		/// <summary>
		/// Рассчитанная дополнительная скидка(не из промонабора)
		/// </summary>
		decimal OurAdditionalDiscountMoney { get; }

		/// <summary>
		/// Сумма в ДВ
		/// </summary>
		decimal OurSum { get; }

		/// <summary>
		/// Названия оснований скидок
		/// </summary>
		string DiscountReasonNames { get; }

		/// <summary>
		/// Тип ошибки заказа
		/// </summary>
		OnlineOrderErrorState? OnlineOrderErrorState { get; }
	}
}
