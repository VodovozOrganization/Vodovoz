using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class PromotionalSetOnlineParametersMap : ClassMap<PromotionalSetOnlineParameters>
	{
		public PromotionalSetOnlineParametersMap()
		{
			Table("promotional_sets_online_parameters");
			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PromotionalSetOnlineAvailability).Column("online_availability");
			Map(x => x.Type).Column("type").Not.Update().Not.Insert().Access.ReadOnly();

			References(x => x.PromotionalSet).Column("promotional_set_id");
		}
	}
}
