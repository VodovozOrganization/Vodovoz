using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Dialogs.Fuel;
using static Vodovoz.ViewModels.Dialogs.Fuel.FuelTypeJournalViewModel;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FuelTypeJournalRegistrar : ColumnsConfigRegistrarBase<FuelTypeJournalViewModel, FuelTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FuelTypeJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(n => n.Id)
				.AddColumn("Название").AddTextRenderer(n => n.Title)
				.AddColumn("")
				.Finish();
	}
}
