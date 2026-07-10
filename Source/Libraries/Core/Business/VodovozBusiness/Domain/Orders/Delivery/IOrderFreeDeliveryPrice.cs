using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	public interface IOrderFreeDeliveryPrice : IFreeDeliveryPrice
	{
		OrderAddressType OrderAddressType { get; }
		int? BottlesReturn { get; }
		IEnumerable<OrderEquipment> ObservableOrderEquipments { get; }
		IEnumerable<OrderDepositItem> ObservableOrderDepositItems { get; }
	}
}
