using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class AiBotNomenclatureOnlinePriceMap : SubclassMap<AiBotNomenclatureOnlinePrice>
	{
		public AiBotNomenclatureOnlinePriceMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForAiBot));
		}
	}
}
