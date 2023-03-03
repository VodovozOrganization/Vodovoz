using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FastDeliveryAvailabilityHistoryJournalRegistrar : ColumnsConfigRegistrarBase<FastDeliveryAvailabilityHistoryJournalViewModel, FastDeliveryAvailabilityHistoryJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FastDeliveryAvailabilityHistoryJournalNode> config) =>
			config.AddColumn("№").AddNumericRenderer(node => node.SequenceNumber)
				.AddColumn("Id").AddNumericRenderer(node => node.Id)
				.AddColumn("Дата и время\nпроверки").AddTextRenderer(node => node.VerificationDateString)
				.AddColumn("Автор заказа").AddTextRenderer(node => node.AuthorString)
				.AddColumn("№ заказа").AddNumericRenderer(node => node.Order)
				.AddColumn("Имя контрагента").AddTextRenderer(node => node.Counterparty)
				.AddColumn("Адрес доставки").AddTextRenderer(node => node.AddressString)
				.AddColumn("Район").AddTextRenderer(node => node.District)
				.AddColumn("Доступно\nдля заказа").AddTextRenderer(node => node.IsValidString)
				.AddColumn("Комментарий логиста /\nПринятые меры").AddTextRenderer(node => node.LogisticianComment)
				.AddColumn("ФИО логиста").AddTextRenderer(node => node.LogisticianNameWithInitials)
				.AddColumn("Дата и время последнего\nсохранения комментария").AddTextRenderer(node => node.LogisticianCommentVersionString)
				.AddColumn("Время реакции в\nчасах : минутах").AddTextRenderer(node => node.LogisticianReactionTime)
				.AddColumn("Ассортимент\nне в запасе").AddTextRenderer(node => node.IsNomenclatureNotInStockSubqueryString)
				.AddColumn("")
				.Finish();
	}
}
