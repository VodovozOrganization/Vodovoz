using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public interface INomenclatureWithDiscountReasons : INomenclatureCount
	{
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; set; }
		/// <summary>
		/// Основания скидок <see cref="Vodovoz.Domain.Orders.DiscountReason"/>
		/// </summary>
		IEnumerable<DiscountReason> DiscountReasons { get; }
	}
}
