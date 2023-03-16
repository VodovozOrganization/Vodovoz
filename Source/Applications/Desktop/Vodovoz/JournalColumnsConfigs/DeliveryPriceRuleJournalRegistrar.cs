using Gamma.ColumnConfig;
using QS.Journal.GtkUI;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DeliveryPriceRuleJournalRegistrar : ColumnsConfigRegistrarBase<DeliveryPriceRuleJournalViewModel, DeliveryPriceRuleJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DeliveryPriceRuleJournalNode> config) =>
			config.AddColumn("<19л б.")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(d => d.Water19LCount.ToString())
					.AddColumn("<6л б.")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(d => d.Water6LCount.ToString())
					.AddColumn("<1,5л б.")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(d => d.Water1500mlCount.ToString())
					.AddColumn("<0,5л б.")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(d => d.Water500mlCount.ToString())
					.AddColumn("Название правила")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(d => d.Name)
					.AddColumn("Описание правила")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(d => d.Description)
					.AddColumn("")
					.Finish();
	}
}
