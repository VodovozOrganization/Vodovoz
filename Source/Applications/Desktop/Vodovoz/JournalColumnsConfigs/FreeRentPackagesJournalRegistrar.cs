using Gamma.ColumnConfig;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FreeRentPackagesJournalRegistrar : ColumnsConfigRegistrarBase<FreeRentPackagesJournalViewModel, FreeRentPackagesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FreeRentPackagesJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.AddTextRenderer(n => n.Name)
				.AddColumn("Вид оборудования")
					.AddTextRenderer(n => n.EquipmentKindName)
				.AddColumn("")
				.Finish();
	}
}
