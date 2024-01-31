using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteColumnMap : ClassMap<RouteColumn>
	{
		public RouteColumnMap()
		{
			Table("nomenclature_route_column");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.ShortName).Column("short_name");
			Map(x => x.IsHighlighted).Column("is_highlighted");
		}
	}
}

