using Gamma.ColumnConfig;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	public class OnlineOrderNotificationSettingJournalRegistrar
		: ColumnsConfigRegistrarBase<OnlineOrderNotificationSettingJournalViewModel, OnlineOrderNotificationSettingJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OnlineOrderNotificationSettingJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Статус онлайн заказ").AddTextRenderer(node => node.ExternalOrderStatus.GetEnumDisplayName(false))
				.AddColumn("Текст уведомления").AddTextRenderer(node => node.NotificationText)
				.AddColumn("")
				.Finish();
	}
}
