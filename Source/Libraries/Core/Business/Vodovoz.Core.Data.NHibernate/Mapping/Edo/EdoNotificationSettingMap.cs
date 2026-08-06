using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoNotificationSettingMap : ClassMap<EdoNotificationSetting>
	{
		public EdoNotificationSettingMap()
		{
			Table("edo_notification_settings");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.EdoNotificationType)
				.Column("edo_notification_type");

			Map(x => x.Template)
				.Column("template");

			Map(x => x.Emails)
				.Column("emails");

			Map(x => x.BitrixDialogs)
				.Column("bitrix_dialogs");

			Map(x => x.NotificationDisabled)
				.Column("notification_disabled");

			Map(x => x.AllowDuplicateNotifications)
				.Column("allow_duplicate_notifications");
		}
	}
}
