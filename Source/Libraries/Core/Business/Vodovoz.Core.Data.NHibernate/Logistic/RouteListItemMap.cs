using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Logistic
{
	public class RouteListItemMap : ClassMap<RouteListItemEntity>
	{
		public RouteListItemMap()
		{
			Table("route_list_addresses");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			OptimisticLock.Version();
			Version(x => x.Version)
				.Column("version");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.UnscannedCodesReason)
				.Column("unscanned_codes_reason");
			
			References(x => x.RouteList)
				.Column("route_list_id")
				.Not.Nullable();

			References(x => x.Order)
				.Column("order_id");

			HasMany(x => x.TrueMarkCodes)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad()
				.KeyColumn("route_list_item_id");
		}
	}
}
