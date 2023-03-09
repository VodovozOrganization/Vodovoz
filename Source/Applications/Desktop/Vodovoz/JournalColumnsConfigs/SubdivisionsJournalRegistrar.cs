using Gamma.ColumnConfig;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SubdivisionsJournalRegistrar : ColumnsConfigRegistrarBase<SubdivisionsJournalViewModel, SubdivisionJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<SubdivisionJournalNode> config) =>
			config.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Руководитель").AddTextRenderer(node => node.ChiefName)
				.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.Finish();
	}
}
