using QS.Project.Journal;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrderNotificationSettingJournalNode : JournalEntityNodeBase<OnlineOrderNotificationSetting>
	{
		public override string Title => $"{ExternalOrderStatus.GetEnumDisplayName()} - {NotificationText}";
		public ExternalOrderStatus ExternalOrderStatus { get; set; }
		public string NotificationText { get; set; }
	}
}
