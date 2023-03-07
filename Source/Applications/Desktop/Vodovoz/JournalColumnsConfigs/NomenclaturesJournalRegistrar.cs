using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NomenclaturesJournalRegistrar : ColumnsConfigRegistrarBase<NomenclaturesJournalViewModel, NomenclatureJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorRed = new Color(0xfe, 0x5c, 0x5c);

		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Номенклатура")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Категория")
					.AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("Кол-во")
					.AddTextRenderer(node => node.InStockText)
				.AddColumn("Зарезервировано")
					.AddTextRenderer(node => node.ReservedText)
				.AddColumn("Доступно")
					.AddTextRenderer(node => node.AvailableText)
					.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? _colorBlack : _colorRed)
				.AddColumn("Код в ИМ")
					.AddTextRenderer(node => node.OnlineStoreExternalId)
				.Finish();
	}
}
