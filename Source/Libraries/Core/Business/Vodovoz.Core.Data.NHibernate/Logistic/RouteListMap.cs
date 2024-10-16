using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Logistic
{
	public class RouteListMap : ClassMap<RouteListEntity>
	{
		public RouteListMap()
		{
			Table("route_lists");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			OptimisticLock.Version();
			Version(x => x.Version)
				.Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date").Access.CamelCaseField(Prefix.Underscore);

			References(x => x.Car).Column("car_id")
				.Access.CamelCaseField(Prefix.Underscore);
			References(x => x.Driver).Column("driver_id")
				.Access.CamelCaseField(Prefix.Underscore);
		}
	}
}
