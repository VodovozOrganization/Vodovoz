using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	/// <summary>
	/// Данные для проверки у заказа бесплатной доставки
	/// </summary>
	public interface IOrderFreeDeliveryPrice : IFreeDeliveryPrice
	{
		/// <summary>
		/// Тип заказа <see cref="Vodovoz.Domain.Orders.OrderAddressType"/>
		/// </summary>
		OrderAddressType OrderAddressType { get; }
		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		int? BottlesReturn { get; }
		/// <summary>
		/// Список оборудования
		/// </summary>
		IEnumerable<OrderEquipment> ObservableOrderEquipments { get; }
		/// <summary>
		/// Список залогов
		/// </summary>
		IEnumerable<OrderDepositItem> ObservableOrderDepositItems { get; }
	}
}
