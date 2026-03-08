using System.Collections.Generic;
using CustomerApps.Contracts.V5;
using Vodovoz.Core.Data.Orders.V5;
using VodovozBusiness.Handlers.V5;

namespace Vodovoz.Application.Orders.Services.V5
{
	/// <summary>
	/// Данные для проверки возможности применения фиксы
	/// </summary>
	public class CanApplyOnlineOrderFixedPriceV5 : ICanApplyOnlineOrderFixedPriceV5
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
		public IEnumerable<IOnlineOrderedProductV5> OnlineOrderItems { get; set; }
	}
}
