using QS.Project.Journal;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrderNotificationSettingJournalNode : JournalEntityNodeBase<OnlineOrderNotificationSetting>
	{
		public override string Title => $"{CustomerNotificationEventType.GetEnumDisplayName()} - {NotificationText}";
		public CustomerNotificationEventType CustomerNotificationEventType { get; set; }
		public string NotificationText { get; set; }
		public bool NotificationDisabled { get; set; }
		public bool AllowDuplicateNotifications { get; internal set; }
	}
}
