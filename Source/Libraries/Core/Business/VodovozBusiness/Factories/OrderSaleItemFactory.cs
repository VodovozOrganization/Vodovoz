using System;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public class OrderSaleItemFactory : IOrderSaleItemFactory
	{
		public OrderItem Create(object source, NewOrderSaleItem newSaleItem)
		{
			return OrderItem.CreateForSale(ThrowIfSourceIsNotOrder(source), newSaleItem);
		}

		public OrderItem CreateNewNonFreeRentDepositItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewNonFreeRentDepositItem(ThrowIfSourceIsNotOrder(source), paidRentPackage);
		}

		public OrderItem CreateNewNonFreeRentServiceItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewNonFreeRentServiceItem(ThrowIfSourceIsNotOrder(source), paidRentPackage);
		}

		public OrderItem CreateNewDailyRentDepositItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewDailyRentDepositItem(ThrowIfSourceIsNotOrder(source), paidRentPackage);
		}

		public OrderItem CreateNewDailyRentServiceItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OrderItem.CreateNewDailyRentServiceItem(ThrowIfSourceIsNotOrder(source), paidRentPackage);
		}

		public OrderItem CreateNewFreeRentDepositItem(
			object source,
			FreeRentPackage freeRentPackage
			)
		{
			return OrderItem.CreateNewFreeRentDepositItem(ThrowIfSourceIsNotOrder(source), freeRentPackage);
		}
		
		private static Order ThrowIfSourceIsNotOrder(object source)
		{
			if(!(source is Order order))
			{
				throw new InvalidOperationException($"Параметр {nameof(source)} должен быть заказом!");
			}

			return order;
		}
	}
}
