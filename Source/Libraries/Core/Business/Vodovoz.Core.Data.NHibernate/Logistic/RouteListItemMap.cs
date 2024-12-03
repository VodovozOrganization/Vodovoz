using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Logistic
{
	public class RouteListItemMap : ClassMap<RouteListItemEntity>
	{
		public RouteListItemMap()
		{
			Table("route_list_addresses");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
		}
	}
}
