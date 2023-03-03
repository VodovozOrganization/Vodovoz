using Gamma.ColumnConfig;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PaidRentPackagesJournalRegistrar : ColumnsConfigRegistrarBase<PaidRentPackagesJournalViewModel, PaidRentPackagesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PaidRentPackagesJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.AddTextRenderer(n => n.Name)
				.AddColumn("Вид оборудования")
					.AddTextRenderer(n => n.EquipmentKindName)
				.AddColumn("Цена в сутки")
					.AddTextRenderer(n => n.PriceDailyString)
				.AddColumn("Цена в месяц")
					.AddTextRenderer(n => n.PriceMonthlyString)
				.AddColumn("")
				.Finish();
	}
}
