using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class UndeliveryDetalizationMap : ClassMap<UndeliveryDetalization>
	{
		public UndeliveryDetalizationMap()
		{
			Table("undelivery_detalizations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.CustomerNotificationText).Column("customer_notification_text");

			References(x => x.UndeliveryKind).Column("undelivery_kind_id");
		}
	}
}
