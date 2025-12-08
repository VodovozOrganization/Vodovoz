using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class KulerSaleWebSitePromotionalSetOnlineParametersMap : SubclassMap<KulerSaleWebSitePromotionalSetOnlineParameters>
	{
		public KulerSaleWebSitePromotionalSetOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForKulerSaleWebSite));
		}
	}
}
