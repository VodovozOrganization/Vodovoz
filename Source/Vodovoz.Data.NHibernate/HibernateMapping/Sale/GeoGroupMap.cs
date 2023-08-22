using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class GeoGroupMap : ClassMap<GeoGroup>
	{
		public GeoGroupMap()
		{
			Table("geo_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			HasMany(x => x.Versions).KeyColumn("geo_group_id").Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}
}
