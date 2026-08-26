using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public interface IGetFixedPrice : INomenclatureCount, IDiscountReasons
	{
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; }
	}
}
