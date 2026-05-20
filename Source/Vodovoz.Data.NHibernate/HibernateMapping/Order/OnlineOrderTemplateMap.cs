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

			Map(x => x.CreatedAt).Column("created_at");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.DeliveryPointId).Column("delivery_point_id");
			Map(x => x.DeliveryScheduleId).Column("delivery_schedule_id");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.BottlesReturn).Column("bottles_return");
			Map(x => x.CallBeforeArrivalMinutes).Column("call_before_arrival_minutes");
			Map(x => x.DontArriveBeforeInterval).Column("dont_arrive_before_interval");
			Map(x => x.Comment).Column("comment");
			Map(x => x.ContactPhone).Column("contact_phone");
			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_id");
			Map(x => x.IsNeedConfirmationByCall).Column("is_need_confirmation_by_call");
			Map(x => x.IsSelfDelivery).Column("is_self_delivery");
			Map(x => x.IsFastDelivery).Column("is_fast_delivery");
			Map(x => x.SelfDeliveryGeoGroupId).Column("self_delivery_geo_group_id");
			Map(x => x.Source).Column("source");
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
