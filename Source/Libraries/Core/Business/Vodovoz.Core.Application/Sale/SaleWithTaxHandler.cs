using Vodovoz.Domain.Service;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleWithTaxHandler : SaleHandler
	{
		public SaleWithTaxHandler(
			SaleItemWithTaxHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator
			) : base(saleItemHandler, goodsPriceCalculator)
		{
		}
	}
}
