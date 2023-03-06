using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DistrictJournalRegistrar : ColumnsConfigRegistrarBase<DistrictJournalViewModel, DistrictJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DistrictJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Зарплатный район").AddTextRenderer(node => node.WageDistrict)
				.AddColumn("Статус версии районов").AddTextRenderer(node => node.DistrictsSetStatus.GetEnumTitle())
				.AddColumn("Код версии").AddNumericRenderer(node => node.DistrictsSetId)
				.AddColumn("")
				.Finish();
	}
}
