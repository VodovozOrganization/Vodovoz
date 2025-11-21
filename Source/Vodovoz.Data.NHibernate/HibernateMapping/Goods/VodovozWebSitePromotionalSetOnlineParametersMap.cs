using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class VodovozWebSitePromotionalSetOnlineParametersMap : SubclassMap<VodovozWebSitePromotionalSetOnlineParameters>
	{
		public VodovozWebSitePromotionalSetOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForVodovozWebSite));
		}
	}
}
