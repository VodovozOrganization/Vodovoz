using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderTemplateWeekdayMap : ClassMap<OnlineOrderTemplateWeekday>
	{
		public OnlineOrderTemplateWeekdayMap()
		{
			Table("online_orders_templates_weekdays");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.TemplateId).Column("template_id").Not.Nullable();
			Map(x => x.Weekday).Column("weekday").Not.Nullable();
			Map(x => x.DayNumber).Column("day_number").Not.Nullable();
		}
	}
}
