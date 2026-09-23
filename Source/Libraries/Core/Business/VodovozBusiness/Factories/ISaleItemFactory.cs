using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public interface ISaleItemFactory
	{
		IProduct Create(object source, NewOrderSaleItem newSaleItem);
		T Create<T>(object source, NewOrderSaleItem newSaleItem);
		IProduct CreateDeliveryOrderItem(object source, Nomenclature nomenclature, decimal price);
		IProduct CreateNewNonFreeRentDepositItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewNonFreeRentServiceItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewDailyRentDepositItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewDailyRentServiceItem(object source, PaidRentPackage paidRentPackage);
		IProduct CreateNewFreeRentDepositItem(object source, FreeRentPackage freeRentPackage);
	}

	public interface IOrderSaleItemFactory
	{
		OrderItem Create(object source, NewOrderSaleItem newSaleItem);
		OrderItem CreateNewNonFreeRentDepositItem(object source, PaidRentPackage paidRentPackage);
		OrderItem CreateNewNonFreeRentServiceItem(object source, PaidRentPackage paidRentPackage);
		OrderItem CreateNewDailyRentDepositItem(object source, PaidRentPackage paidRentPackage);
		OrderItem CreateNewDailyRentServiceItem(object source, PaidRentPackage paidRentPackage);
		OrderItem CreateNewFreeRentDepositItem(object source, FreeRentPackage freeRentPackage);
	}
}
