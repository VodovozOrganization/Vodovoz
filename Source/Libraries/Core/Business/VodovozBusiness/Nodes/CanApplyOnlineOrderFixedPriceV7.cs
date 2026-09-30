using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Данные для проверки возможности применения фиксы
	/// </summary>
	public class CanApplyOnlineOrderFixedPriceV7
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
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum => OnlineOrderItems.Sum(x => x.CurrentSum);
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<IOrderedCartItem> OnlineOrderItems { get; set; }
	}
}
