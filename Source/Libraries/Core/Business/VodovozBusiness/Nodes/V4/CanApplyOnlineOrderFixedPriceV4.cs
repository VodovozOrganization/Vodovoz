using System.Collections.Generic;
<<<<<<<< HEAD:Source/Libraries/Core/Backend/CustomerOrders.Contracts/CanApplyOnlineOrderFixedPrice.cs
using CustomerOrders.Contracts.Interfaces;

namespace CustomerOrders.Contracts
========
using Vodovoz.Core.Data.V4;

namespace VodovozBusiness.Nodes.V4
>>>>>>>> origin/5696_AddCreatingOnlineOrderFromTemplate:Source/Libraries/Core/Business/VodovozBusiness/Nodes/V4/CanApplyOnlineOrderFixedPriceV4.cs
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
