using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class OrderWithoutShipmentDiscountController : DiscountWithTaxController
	{
		protected OrderWithoutShipmentDiscountController(SaleItemTaxHandler taxHandler) : base(taxHandler)
		{
			
		}
		
		//TODO проверить работу установки скидки, должны устанавливаться все параметры(скидка деньгами, проценты и булево)
		/*protected override void RecalculateTotalDiscountFromReasons(IApplyDiscountReason saleItem)
		{
			var currentPrice = saleItem.CurrentRawPrice;
			var totalDiscountMoney = CalculateTotalDiscountInMoneyFromAddedReasons(saleItem);

			if(totalDiscountMoney > currentPrice)
			{
				totalDiscountMoney = currentPrice;
			}

			
			DiscountMoney = totalDiscountMoney;
			Discount = currentPrice > 0 ? (100 * DiscountMoney) / currentPrice : 0;
			
			TaxHandler.RecalculateTaxSum(saleItem);
		}*/
	}
}
