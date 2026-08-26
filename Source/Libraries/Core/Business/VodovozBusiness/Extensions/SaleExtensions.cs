using System;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Extensions
{
	public static class SaleExtensions
	{
		public static IOrderSaleItem ToOrderSaleItem(this ISaleItem saleItem)
		{
			if(!(saleItem is IOrderSaleItem item))
			{
				throw new InvalidOperationException($"{saleItem.GetType()} должен реализовывать {nameof(IOrderSaleItem)}");
			}
			
			return item;
		}
		
		public static IPreserveDiscount ToPreserveDiscount(this ISaleItem saleItem)
		{
			if(!(saleItem is IPreserveDiscount discountItem))
			{
				throw new InvalidOperationException(
					$"{saleItem.GetType()} должен реализовывать {nameof(IPreserveDiscount)}, чтобы пересчитать скидку");
			}
			
			return discountItem;
		}
		
		public static IApplyDiscountReasonItem ToApplyDiscountReasonItem(this ISaleItem saleItem)
		{
			if(!(saleItem is IApplyDiscountReasonItem discountItem))
			{
				throw new InvalidOperationException(
					$"{saleItem.GetType()} должен реализовывать {nameof(IApplyDiscountReasonItem)}, чтобы пересчитать скидку");
			}
			
			return discountItem;
		}
	}
}
