using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
	public class FlyerMap : ClassMap<Flyer>
	{
		public FlyerMap()
		{
			Table("leaflets");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.FlyerNomenclature).Column("leaflet_nomenclature_id");
			HasMany(x => x.FlyerActionTimes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("leaflet_id");
		}
	}
}