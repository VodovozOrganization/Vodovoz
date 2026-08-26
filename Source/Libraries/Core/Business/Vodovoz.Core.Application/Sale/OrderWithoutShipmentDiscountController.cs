using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class OrderWithoutShipmentDiscountController : DiscountController
	{
		
		//TODO проверить работу установки скидки, должны устанавливаться все параметры(скидка деньгами, проценты и булево)
		public OrderWithoutShipmentDiscountController(IDiscountReasonRepository discountReasonRepository, IDiscountReasonSettings discountReasonSettings) : base(discountReasonRepository, discountReasonSettings)
		{
		}

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
