using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public interface IRecalculateRentCount : INomenclatureCount
	{
		int RentEquipmentCount { get; set; }
		int RentCount { get; set; }
		OrderItemRentSubType OrderItemRentSubType { get; }
	}
}
