using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarManufacturerJournalRegistrar : ColumnsConfigRegistrarBase<CarManufacturerJournalViewModel, CarManufacturerJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CarManufacturerJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Title)
				.AddColumn("")
				.Finish();
	}
}
