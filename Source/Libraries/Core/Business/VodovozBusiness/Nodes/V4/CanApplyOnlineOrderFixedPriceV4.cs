using System.Collections.Generic;
using Vodovoz.Core.Data.Orders.V4;

namespace VodovozBusiness.Nodes.V4
{
	/// <summary>
	/// Данные для проверки возможности применения фиксы
	/// </summary>
	public class CanApplyOnlineOrderFixedPriceV4
	{
		/// <summary>
		/// Id клиента
		/// </summary>
		public int? CounterpartyId { get; set; }
		/// <summary>
		/// Id точки доставки
		/// </summary>
		public int? DeliveryPointId { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<IOnlineOrderedProductV4> OnlineOrderItems { get; set; }
	}
}
