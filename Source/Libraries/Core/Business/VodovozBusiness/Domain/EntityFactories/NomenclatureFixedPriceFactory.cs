using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.EntityFactories 
{
	public class NomenclatureFixedPriceFactory : INomenclatureFixedPriceFactory
	{
		public NomenclatureFixedPrice Create()
		{
			return new NomenclatureFixedPrice();
		}
	}
}
