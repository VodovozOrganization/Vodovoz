using System;
using Vodovoz.Core.Application.Extensions;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using VodovozBusiness.Controllers;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleItemWithTaxHandler : SaleItemHandler
	{
		public SaleItemWithTaxHandler(
			IDiscountController discountController,
			ISaleItemTaxHandler saleItemTaxHandler
		) : base(discountController)
		{
			TaxHandler = saleItemTaxHandler ?? throw new ArgumentNullException(nameof(saleItemTaxHandler));
		}

		protected ISaleItemTaxHandler TaxHandler { get; }
		
		internal virtual void SetPriceForNewSaleItem(IDataContext context, decimal price)
		{
			var saleItem = context
				.ContextDataToCommonRecalculateDiscount()
				.SaleItem;
			
			SetPriceWithoutRecalculate(saleItem, price);
			RecalculateDiscountAndSetTax(context);
		}

		internal override void RecalculateDiscounts(IDataContext context)
		{
			var saleItem = context
				.ContextDataToCommonRecalculateDiscount()
				.SaleItem;
			
			base.RecalculateDiscounts(context);
			TaxHandler.RecalculateTaxSum(saleItem as IRecalculateTax);
		}
		
		private void RecalculateDiscountAndSetTax(IDataContext context)
		{
			var saleItem = context
				.ContextDataToCommonRecalculateDiscount()
				.SaleItem;

			base.RecalculateDiscounts(context);
			TaxHandler.CalculateTax(saleItem as IRecalculateTax);
		}
	}
}
