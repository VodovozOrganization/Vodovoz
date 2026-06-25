using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public class OrderSaleItemFactory : ISaleItemFactory
	{
		public IProduct Create(
			object source,
			decimal count,
			decimal price, //nomenclature.GetPrice(1, canApplyAlternativePrice)
			Nomenclature nomenclature
		)
		{
			return OrderItem.CreateForSale(source as Order, nomenclature, count, price);
		}

		public IProduct CreateDeliveryOrderItem(
			object source,
			Nomenclature nomenclature,
			decimal price
			)
		{
			return OrderItem.CreateDeliveryOrderItem(source as Order, nomenclature, price);
		}

		public IProduct CreateNewNonFreeRentDepositItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewNonFreeRentDepositItem(source as Order, paidRentPackage);
		}

		public IProduct CreateNewNonFreeRentServiceItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewNonFreeRentServiceItem(source as Order, paidRentPackage);
		}

		public IProduct CreateNewDailyRentDepositItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewDailyRentDepositItem(source as Order, paidRentPackage);
		}

		public IProduct CreateNewDailyRentServiceItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewDailyRentServiceItem(source as Order, paidRentPackage);
		}

		public IProduct CreateNewFreeRentDepositItem(
			object source,
			FreeRentPackage freeRentPackage
			)
		{
			return OrderItem.CreateNewFreeRentDepositItem(source as Order, freeRentPackage);
		}
	}
}
