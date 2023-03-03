using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DeliveryPointResponsiblePersonTypeJournalRegistrar : ColumnsConfigRegistrarBase<DeliveryPointResponsiblePersonTypeJournalViewModel, DeliveryPointResponsiblePersonTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DeliveryPointResponsiblePersonTypeJournalNode> config) =>
			config.AddColumn("Номер")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Имя")
					.AddTextRenderer(node => node.Title)
				.AddColumn("")
				.Finish();
	}
}
