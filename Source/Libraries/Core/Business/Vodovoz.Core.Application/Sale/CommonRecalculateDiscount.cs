using System;
using Vodovoz.Core.Application.Common;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class CommonRecalculateDiscount
	{
		protected CommonRecalculateDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue)
		{
			SaleItem = saleItem
				?? throw new ArgumentNullException(
					nameof(saleItem),
					$"Продаваемая позиция в {nameof(CommonRecalculateDiscount)} не может быть пустой");
			DiscountValue = discountValue
				?? throw new ArgumentNullException(
					nameof(discountValue),
					$"Значение скидки в {nameof(CommonRecalculateDiscount)} не может быть пустым");
		}

		public IApplyDiscountReasonItem SaleItem { get; }
		public IDiscountValue DiscountValue { get; }
		
		public static IDataContext CreateDataContext(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue) =>
			DataContext.Create(Create(saleItem, discountValue));
		
		public static IDataContext CreateDataContext(IApplyDiscountReasonItem saleItem) =>
			DataContext.Create(Create(saleItem, saleItem.DiscountData));
		
		public static CommonRecalculateDiscount Create(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue) =>
			new CommonRecalculateDiscount(saleItem, discountValue);
	}
}
