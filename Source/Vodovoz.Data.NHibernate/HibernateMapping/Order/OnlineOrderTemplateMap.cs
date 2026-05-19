using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderTemplateMap : ClassMap<OnlineOrderTemplate>
	{
		public OnlineOrderTemplateMap()
		{
			Table("online_orders_templates");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.DeliveryPointId).Column("delivery_point_id");
			Map(x => x.CreatedAt).Column("created_at");
			Map(x => x.DeliveryScheduleId).Column("delivery_schedule_id");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.PaymentType).Column("payment_type");
			Map(x => x.DeliveryFrequency).Column("delivery_frequency");
			
			HasMany(x => x.TemplateProducts)
				.Table("online_orders_templates_products")
				.KeyColumn("template_id")
				.Element("id")
				;
			
			HasMany(x => x.Weekdays)
				.Table("online_orders_templates_weekdays")
				.KeyColumn("template_id")
				.Element("id")
				;
		}
	}
}
