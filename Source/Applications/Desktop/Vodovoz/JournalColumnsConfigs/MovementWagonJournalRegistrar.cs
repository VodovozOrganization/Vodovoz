using Gamma.ColumnConfig;
using Vodovoz.JournalNodes;
using Vodovoz.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class MovementWagonJournalRegistrar : ColumnsConfigRegistrarBase<MovementWagonJournalViewModel, MovementWagonJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<MovementWagonJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
	}
}
