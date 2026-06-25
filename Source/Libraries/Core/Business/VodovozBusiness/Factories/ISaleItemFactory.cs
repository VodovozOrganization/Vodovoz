using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public interface ISaleItemFactory
	{
		IProduct Create(object source, decimal count, decimal price, Nomenclature nomenclature);
		IProduct CreateDeliveryOrderItem(object source, Nomenclature nomenclature, decimal price);
		IProduct CreateNewNonFreeRentDepositItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewNonFreeRentServiceItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewDailyRentDepositItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewDailyRentServiceItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewFreeRentDepositItem(object source, FreeRentPackage freeRentPackage);
	}
}
