using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.HibernateMapping.Goods
{
	public class VodovozWebSiteNomenclatureOnlinePriceMap : SubclassMap<VodovozWebSiteNomenclatureOnlinePrice>
	{
		public VodovozWebSiteNomenclatureOnlinePriceMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForVodovozWebSite));
		}
	}
}
