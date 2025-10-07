using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class FineMap : ClassMap<Fine>
	{
		public FineMap()
		{
			Table("fines");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.TotalMoney).Column("total_money");
			Map(x => x.FineReasonString).Column("fine_reason_string");
			Map(x => x.FineType).Column("fine_type");
			Map(x => x.FineCategory).Column("fine_category");

			References(x => x.RouteList).Column("route_list_id");
			References(x => x.Author).Column("author_id");
			References(x => x.UndeliveredOrder).Column("undelivery_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("fine_id");
			HasMany(x => x.Nomenclatures).Cascade.AllDeleteOrphan().Inverse().KeyColumn("fine_id");

			HasManyToMany(x => x.RouteListItems)
				.Table("fines_to_route_list_addresses")
				.ParentKeyColumn("fine_id")
				.ChildKeyColumn("route_list_address_id")
				.LazyLoad();
		}
	}
}
