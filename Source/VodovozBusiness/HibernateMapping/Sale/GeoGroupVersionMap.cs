using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Sale
{
	public class GeoGroupVersionMap : ClassMap<GeoGroupVersion>
	{
		public GeoGroupVersionMap()
		{
			Table("geo_group_versions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.GeoGroup).Column("geo_group_id");
			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.ActivationDate).Column("activation_date");
			Map(x => x.ClosingDate).Column("closing_date");
			Map(x => x.Status).Column("status");
			References(x => x.Author).Column("author_id");
			Map(x => x.BaseLatitude).Column("latitude");
			Map(x => x.BaseLongitude).Column("longitude");
			References(x => x.CashSubdivision).Column("cash_subdivision_id");
			References(x => x.Warehouse).Column("warehouse_id");
		}
	}
}
