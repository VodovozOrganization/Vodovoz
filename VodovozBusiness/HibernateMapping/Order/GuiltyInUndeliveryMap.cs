using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class GuiltyInUndeliveryMap : ClassMap<GuiltyInUndelivery>
	{
		public GuiltyInUndeliveryMap()
		{
			Table("guilty_in_undelivered_orders");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.GuiltySide).Column("guilty_side").CustomType<GuiltyInUndeliveryGuiltySideStringType>();

			References(x => x.UndeliveredOrder).Column("undelivery_id");
			References(x => x.GuiltyDepartment).Column("guilty_department_id");
		}
	}
}
