using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.HibernateMapping.Goods
{
	public class KulerSaleWebSiteNomenclatureOnlinePriceMap : SubclassMap<KulerSaleWebSiteNomenclatureOnlinePrice>
	{
		public KulerSaleWebSiteNomenclatureOnlinePriceMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForKulerSaleWebSite));
		}
	}
}
