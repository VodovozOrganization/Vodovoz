using System.Collections.Generic;
using Vodovoz.Core.Data.V5;

namespace VodovozBusiness.Handlers.V5
{
	/// <summary>
	/// Контракт проверки применимости фиксы к онлайн заказу
	/// </summary>
	public interface ICanApplyOnlineOrderFixedPriceV5
	{
		/// <summary>
		/// Id клиента
		/// </summary>
		int? CounterpartyId { get; }
		/// <summary>
		/// Id точки доставки
		/// </summary>
		int? DeliveryPointId { get; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		bool IsSelfDelivery { get; }
		/// <summary>
		/// Список товаров
		/// </summary>
		IEnumerable<IOnlineOrderedProductV5> OnlineOrderItems { get; }
	}
}
