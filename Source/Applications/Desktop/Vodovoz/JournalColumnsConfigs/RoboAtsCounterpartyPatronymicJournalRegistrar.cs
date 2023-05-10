using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RoboAtsCounterpartyPatronymicJournalRegistrar : ColumnsConfigRegistrarBase<RoboAtsCounterpartyPatronymicJournalViewModel, RoboAtsCounterpartyPatronymicJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RoboAtsCounterpartyPatronymicJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Отчество").AddTextRenderer(node => node.Patronymic)
				.AddColumn("Ударение").AddTextRenderer(node => node.Accent)
				.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats.ConvertToYesOrEmpty())
				.Finish();
	}
}
