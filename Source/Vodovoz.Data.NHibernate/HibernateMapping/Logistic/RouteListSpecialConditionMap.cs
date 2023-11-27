using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteListSpecialConditionMap : ClassMap<RouteListSpecialCondition>
	{
		public RouteListSpecialConditionMap()
		{
			Table("route_list_special_conditions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.RouteListId).Column("route_list_id");
			Map(x => x.RouteListSpecialConditionTypeId).Column("route_list_special_condition_type_id");
			Map(x => x.Accepted).Column("accepted");
			Map(x => x.CreatedAt).Column("created_at").ReadOnly();
		}
	}
}
