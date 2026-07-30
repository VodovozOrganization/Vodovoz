using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class LastServiceOrderMap : ClassMap<LastServiceOrder>
	{
		public LastServiceOrderMap()
		{
			Table("last_service_orders");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.Stage).Column("stage");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.CounterpartyName).Column("counterparty_name");
			Map(x => x.DeliveryPointId).Column("delivery_point_id");
			Map(x => x.IsSelfDelivery).Column("is_self_delivery");
			Map(x => x.DeliveryPointAddress).Column("delivery_point_address");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.EmailAddress).Column("email_address");
			Map(x => x.LastOrderId).Column("last_order_id");
			Map(x => x.LastOrderDeliveryDate).Column("last_order_delivery_date");
		}
	}
}
