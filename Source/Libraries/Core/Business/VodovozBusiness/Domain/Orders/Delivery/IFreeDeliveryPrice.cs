using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	/// <summary>
	/// Контракт проверки бесплатной доставки
	/// </summary>
	public interface IFreeDeliveryPrice : ISaleItems
	{
		/// <summary>
		/// Точка доставки
		/// </summary>
		DeliveryPoint DeliveryPoint { get; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		bool IsSelfDelivery { get; }
	}
}
