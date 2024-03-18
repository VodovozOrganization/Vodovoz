using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteListSpecialConditionTypeMap : ClassMap<RouteListSpecialConditionType>
	{
		public RouteListSpecialConditionTypeMap()
		{
			Table("route_list_special_condition_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
