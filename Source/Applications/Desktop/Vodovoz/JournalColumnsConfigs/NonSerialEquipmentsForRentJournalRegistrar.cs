using Gamma.ColumnConfig;
using Gdk;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NonSerialEquipmentsForRentJournalRegistrar : ColumnsConfigRegistrarBase<NonSerialEquipmentsForRentJournalViewModel, NomenclatureForRentNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorRed = new Color(0xfe, 0x5c, 0x5c);

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
					.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? _colorBlack : _colorRed)
				.AddColumn("")
				.Finish();
	}
}
