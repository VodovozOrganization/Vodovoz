using System.Collections.Generic;
using Vodovoz.Core.Data.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Данные для проверки возможности применения фиксы
	/// </summary>
	public class CanApplyOnlineOrderFixedPrice
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
		public IEnumerable<IOnlineOrderedProduct> OnlineOrderItems { get; set; }
	}
}
