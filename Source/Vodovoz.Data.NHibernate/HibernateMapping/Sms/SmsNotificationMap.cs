using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sms;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sms
{
	public class SmsNotificationMap : ClassMap<SmsNotification>
	{
		public SmsNotificationMap()
		{
			Table("sms_notifications");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");
			Map(x => x.Status).Column("status");
			Map(x => x.MobilePhone).Column("phone");
			Map(x => x.MessageText).Column("message_text");
			Map(x => x.ErrorDescription).Column("error_description");
			Map(x => x.NotifyTime).Column("notify_time");
			Map(x => x.ExpiredTime).Column("expired_time");
			Map(x => x.ServerMessageId).Column("server_id");
			Map(x => x.Description).Column("description");
		}
	}

	public class NewClientSmsNotificationMap : SubclassMap<NewClientSmsNotification>
	{
		public NewClientSmsNotificationMap()
		{
			DiscriminatorValue(nameof(SmsNotificationType.NewClient));
			References(x => x.Order).Column("order_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}

	public class LowBalanceSmsNotificationMap : SubclassMap<LowBalanceSmsNotification>
	{
		public LowBalanceSmsNotificationMap()
		{
			DiscriminatorValue(nameof(SmsNotificationType.LowBalance));
			Map(x => x.Balance).Column("balance");
		}
	}

	public class UndeliveryNotApprovedSmsNotificationMap : SubclassMap<UndeliveryNotApprovedSmsNotification>
	{
		public UndeliveryNotApprovedSmsNotificationMap()
		{
			DiscriminatorValue(nameof(SmsNotificationType.UndeliveryNotApproved));
			References(x => x.UndeliveredOrder).Column("undeliveried_order_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}
