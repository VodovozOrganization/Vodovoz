using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteListDebtMap : ClassMap<RouteListDebt>
	{
		public RouteListDebtMap()
		{
			Table("route_lists_debts");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.RouteList).Column("route_list_id");

			Map(x => x.Debt).Column("debt");
		}
	}
}
