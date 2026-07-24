using Gamma.ColumnConfig;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;

namespace Vodovoz.JournalColumnsConfigs
{
	public class EdoNotificationSettingJournalRegistrar
		: ColumnsConfigRegistrarBase<EdoNotificationSettingJournalViewModel, EdoNotificationSettingJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoNotificationSettingJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Тип уведомления").AddTextRenderer(node => node.EdoNotificationType.GetEnumDisplayName(false))
				.AddColumn("Шаблон уведомления").AddTextRenderer(node => node.Template)
				.AddColumn("Emails").AddTextRenderer(node => node.Emails)
				.AddColumn("Диалоги Битрикс").AddTextRenderer(node => node.BitrixDialogs)
				.AddColumn("Не отправлять").AddToggleRenderer(node => node.NotificationDisabled).Editing(false)
				.AddColumn("Разрешить повторные отправки").AddToggleRenderer(node => node.AllowDuplicateNotifications).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
