using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class OnlineOrderStatusUpdatedNotificationMap : ClassMap<OnlineOrderStatusUpdatedNotification>
	{
		public OnlineOrderStatusUpdatedNotificationMap()
		{
			Table("online_orders_status_updated_notifications");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.HttpCode).Column("http_code");
			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.SentDate).Column("sent_date");

			References(x => x.OnlineOrder).Column("online_order_id");
		}
	}
}
