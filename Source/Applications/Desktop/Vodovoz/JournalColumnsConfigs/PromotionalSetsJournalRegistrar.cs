using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PromotionalSetsJournalRegistrar : ColumnsConfigRegistrarBase<PromotionalSetsJournalViewModel, PromotionalSetJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGrey = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<PromotionalSetJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PromoSetDiscountReasonName)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddTextRenderer()
					.AddSetter((c, n) => c.Text = n.IsArchive ? "Да" : string.Empty)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
				.Finish();
	}
}
