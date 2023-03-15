using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarModelJournalRegistrar : ColumnsConfigRegistrarBase<CarModelJournalViewModel, CarModelJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGrey = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<CarModelJournalNode> config) =>
			config.AddColumn("Код").HeaderAlignment(0.5f).AddTextRenderer(n => n.Id.ToString()).XAlign(0.5f)
				.AddColumn("Производитель").HeaderAlignment(0.5f).AddTextRenderer(n => n.ManufactererName).XAlign(0.5f)
				.AddColumn("Название").HeaderAlignment(0.5f).AddTextRenderer(n => n.Name).XAlign(0.5f)
				.AddColumn("Тип").HeaderAlignment(0.5f).AddTextRenderer(n => n.TypeOfUse.GetEnumTitle()).XAlign(0.5f)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
				.Finish();
	}
}
