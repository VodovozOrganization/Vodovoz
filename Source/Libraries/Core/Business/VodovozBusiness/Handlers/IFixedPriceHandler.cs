using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Handlers
{
	public interface IFixedPriceHandler
	{
		decimal? GetWaterFixedPrice(
			IGetFixedPriceSource addProductSource,
			INomenclatureCount addingItem
		);

		NomenclatureFixedPrice GetFixedPriceOrNull(
			IGetFixedPriceSource addProductSource,
			INomenclatureCount addingItem,
			decimal count
		);
	}
}
