using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RoboAtsCounterpartyNameJournalRegistrar : ColumnsConfigRegistrarBase<RoboAtsCounterpartyNameJournalViewModel, RoboAtsCounterpartyNameJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RoboAtsCounterpartyNameJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Имя").AddTextRenderer(node => node.Name)
				.AddColumn("Ударение").AddTextRenderer(node => node.Accent)
				.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats.ConvertToYesOrEmpty())
				.Finish();
	}
}
