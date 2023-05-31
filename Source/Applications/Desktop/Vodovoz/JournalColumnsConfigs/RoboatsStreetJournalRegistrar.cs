using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class RoboatsStreetJournalRegistrar : ColumnsConfigRegistrarBase<RoboatsStreetJournalViewModel, RoboatsStreetJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RoboatsStreetJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Улица").AddTextRenderer(node => node.Name)
				.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats.ConvertToYesOrEmpty())
				.AddColumn("")
				.Finish();
	}
}
