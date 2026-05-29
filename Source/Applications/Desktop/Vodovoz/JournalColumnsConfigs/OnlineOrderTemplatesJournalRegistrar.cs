using Gamma.ColumnConfig;
using Pango;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	public class OnlineOrderTemplatesJournalRegistrar :
		ColumnsConfigRegistrarBase<OnlineOrderTemplatesJournalViewModel, OnlineOrderTemplatesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OnlineOrderTemplatesJournalNode> config) =>
			config
				.AddColumn("Номер")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Клиент")
					.AddTextRenderer(node => node.CounterpartyName)
					.WrapMode(WrapMode.Word)
					.WrapWidth(350)
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.CompiledAddress)
					.WrapMode(WrapMode.Word)
					.WrapWidth(500)
				.AddColumn("Телефон")
					.AddTextRenderer(node => node.ContactPhone)
				.AddColumn("Активен")
					.AddTextRenderer(node => node.IsActive.ConvertToYesOrNo())
				.AddColumn("Дни недели")
					.AddTextRenderer(node => node.Weekdays)
				.AddColumn("Время доставки")
					.AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
				.AddColumn("Периодичность доставки")
					.AddTextRenderer(node => node.DeliveryFrequency.GetEnumDisplayName(false))
				.AddColumn("Последний заказ")
					.AddTextRenderer(node => node.LastOnlineOrderIdFromTemplate.HasValue ? node.LastOnlineOrderIdFromTemplate.ToString() : string.Empty)
				.AddColumn("В архиве")
					.AddTextRenderer(node => node.IsArchive.ConvertToYesOrNo())
				.Finish();
	}
}
