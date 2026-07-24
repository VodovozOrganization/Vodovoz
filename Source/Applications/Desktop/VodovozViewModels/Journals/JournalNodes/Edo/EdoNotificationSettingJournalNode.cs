using EdoNotifications.Contracts;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	public class EdoNotificationSettingJournalNode : JournalEntityNodeBase<EdoNotificationSetting>
	{
		public override string Title => $"{EdoNotificationType.GetEnumDisplayName()}";
		public EdoNotificationType EdoNotificationType { get; set; }
		public string Template { get; set; }
		public bool NotificationDisabled { get; set; }
		public bool AllowDuplicateNotifications { get; internal set; }
		public string Emails { get; set; }
		public string BitrixDialogs { get; set; }
	}
}
