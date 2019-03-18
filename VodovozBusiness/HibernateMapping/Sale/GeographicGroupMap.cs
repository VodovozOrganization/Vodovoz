using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Sale
{
	public class GeographicGroupMap : ClassMap<GeographicGroup>
	{
		public GeographicGroupMap()
		{
			Table("geographic_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.BaseLatitude).Column("latitude");
			Map(x => x.BaseLongitude).Column("longitude");
		}
	}
}
