using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarJournalRegistrar : ColumnsConfigRegistrarBase<CarJournalViewModel, CarJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<CarJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Производитель").AddTextRenderer(x => x.ManufacturerName).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Модель").AddTextRenderer(x => x.ModelName).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Гос. номер").AddTextRenderer(x => x.RegistrationNumber)
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverName)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGray : _colorBlack)
				.Finish()
	}
}
