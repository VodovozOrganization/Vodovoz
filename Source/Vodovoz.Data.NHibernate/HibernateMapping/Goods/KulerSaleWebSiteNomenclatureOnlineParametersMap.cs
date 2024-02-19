using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.HibernateMapping.Goods
{
	public class KulerSaleWebSiteNomenclatureOnlineParametersMap : SubclassMap<KulerSaleWebSiteNomenclatureOnlineParameters>
	{
		public KulerSaleWebSiteNomenclatureOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForKulerSaleWebSite));
		}
	}
}
