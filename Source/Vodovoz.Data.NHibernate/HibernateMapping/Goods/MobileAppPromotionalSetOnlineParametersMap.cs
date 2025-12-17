using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class MobileAppPromotionalSetOnlineParametersMap : SubclassMap<MobileAppPromotionalSetOnlineParameters>
	{
		public MobileAppPromotionalSetOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForMobileApp));
		}
	}
}
