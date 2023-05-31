using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NomenclatureStockBalanceJournalRegistrar : ColumnsConfigRegistrarBase<NomenclatureStockBalanceJournalViewModel, NomenclatureStockJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureStockJournalNode> config) =>
			config.AddColumn("Код").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Номенклатура").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureName)
				.AddColumn("Кол-во").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.AmountText).XAlign(0.5f)
				.AddColumn("Мин кол-во\n на складе").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.MinCountText).XAlign(0.5f)
				.AddColumn("Разница").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DiffCountText).XAlign(0.5f)
				.AddColumn("Экземплярный учет")
					.AddTextRenderer(node => node.HasInventoryAccounting ? "Да" : string.Empty)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					var color = GdkColors.BlackColor;
					if(n.StockAmount < 0)
					{
						color = new Color(255, 30, 30);
					}
					c.ForegroundGdk = color;
				})
				.Finish();
	}
}
