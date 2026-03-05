using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

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
			Map(x => x.RepeatOrder).Column("repeat_order");
			
			HasMany(x => x.TemplateProducts)
				.Table("online_orders_templates_products")
				.KeyColumn("template_id")
				.Element("id")
				;
			
			/*HasManyToMany(x => x.TemplateProducts)
				.Table("online_orders_templates_to_products")
				.ParentKeyColumn("template_id")
				.ChildKeyColumn("product_id")
				.LazyLoad();*/
		}
	}
}
