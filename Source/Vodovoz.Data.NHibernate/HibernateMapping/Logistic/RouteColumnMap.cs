using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class RouteColumnMap : ClassMap<RouteColumn>
	{
		public RouteColumnMap()
		{
			Table("nomenclature_route_column");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}

