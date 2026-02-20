using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class AiBotPromotionalSetOnlineParametersMap : SubclassMap<AiBotPromotionalSetOnlineParameters>
	{
		public AiBotPromotionalSetOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForAiBot));
		}
	}
}
