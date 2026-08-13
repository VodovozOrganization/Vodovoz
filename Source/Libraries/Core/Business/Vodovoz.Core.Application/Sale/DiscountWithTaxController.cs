using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class DiscountWithTaxController : DiscountController
	{
		protected DiscountWithTaxController(
			SaleItemTaxHandler taxHandler
			)
		{
			TaxHandler = taxHandler ?? throw new ArgumentNullException(nameof(taxHandler));
		}
		
		protected SaleItemTaxHandler TaxHandler { get; }

		protected override void RecalculateTotalDiscountFromReasons(IApplyDiscountReasonItem saleItem)
		{
			if(saleItem is not IRecalculateTax recalculateTaxItem)
			{
				throw new InvalidOperationException();
			}
			
			base.RecalculateTotalDiscountFromReasons(saleItem);
			TaxHandler.RecalculateTaxSum(recalculateTaxItem);
		}
	}
}
