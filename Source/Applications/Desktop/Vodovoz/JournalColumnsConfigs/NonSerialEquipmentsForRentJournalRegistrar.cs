using Gamma.ColumnConfig;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NonSerialEquipmentsForRentJournalRegistrar : ColumnsConfigRegistrarBase<NonSerialEquipmentsForRentJournalViewModel, NomenclatureForRentNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureForRentNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Оборудование")
					.AddTextRenderer(node => node.NomenclatureName)
				.AddColumn("Вид оборудования")
					.AddTextRenderer(node => node.EquipmentKindName)
				.AddColumn("Кол-во")
					.AddTextRenderer(node => node.InStockText)
				.AddColumn("Зарезервировано")
					.AddTextRenderer(node => node.ReservedText)
				.AddColumn("Доступно")
					.AddTextRenderer(node => node.AvailableText)
					.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? GdkColors.PrimaryText : GdkColors.Red2)
				.AddColumn("")
				.Finish();
	}
}
