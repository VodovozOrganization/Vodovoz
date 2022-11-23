using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
	public class FlyerMap : ClassMap<Flyer>
	{
		public FlyerMap()
		{
			Table("flyers");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.IsForFirstOrder).Column("is_for_first_order");

			References(x => x.FlyerNomenclature).Column("flyer_nomenclature_id");
			
			HasMany(x => x.FlyerActionTimes).Cascade.AllDeleteOrphan().Inverse().KeyColumn("flyer_id");
		}
	}
}
