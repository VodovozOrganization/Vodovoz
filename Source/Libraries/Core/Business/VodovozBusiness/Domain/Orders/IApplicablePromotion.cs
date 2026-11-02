using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Domain.Orders
{
	public interface IApplicablePromotion : ICurrentRawPrice, IDiscountReasons
	{
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Номенклатура
		/// </summary>
		Nomenclature Nomenclature { get; }
		/// <summary>
		/// Промонабор
		/// </summary>
		PromotionalSet PromoSet { get; }
	}
}
