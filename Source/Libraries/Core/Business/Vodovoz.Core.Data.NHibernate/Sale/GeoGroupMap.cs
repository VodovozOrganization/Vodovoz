using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Core.Data.NHibernate.Sale
{
	public class GeoGroupMap : ClassMap<GeoGroupEntity>
	{
		public GeoGroupMap()
		{
			Table("geo_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
