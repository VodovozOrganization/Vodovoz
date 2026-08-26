using System;
using Vodovoz.Core.Application.Extensions;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleItemHandler
	{
		public SaleItemHandler(
			IDiscountController discountController
			)
		{
			DiscountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
		}
		
		protected IDiscountController DiscountController { get; }

		internal virtual void RecalculateDiscounts(IDataContext context)
		{
			DiscountController.RecalculateDiscount(context);
		}
		
		internal virtual bool SetCount(INomenclatureCount saleItem, decimal count)
		{
			if(saleItem.Nomenclature?.Unit?.Digits == 0 && count % 1 != 0)
			{
				count = Math.Truncate(count);
			}

			if(saleItem.Count == count)
			{
				return false;
			}

			saleItem.Count = count < 0 ? 0 : count;
			return true;
		}
		
		internal virtual bool SetPrice(IDataContext context, decimal price)
		{
			var saleItem = context
				.ContextDataToCommonRecalculateDiscount()
				.SaleItem;
			
			if(!SetPriceWithoutRecalculate(saleItem, price))
			{
				return false;
			}
				
			RecalculateDiscounts(context);
			return true;
		}
		
		protected virtual bool SetPriceWithoutRecalculate(IPrice saleItem, decimal price)
		{
			price = decimal.Round(price, 2);

			if(saleItem.Price == price)
			{
				return false;
			}
			
			saleItem.Price = price;
			return true;
		}
	}
}
