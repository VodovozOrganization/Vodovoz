using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ComplaintKindJournalRegistrar : ColumnsConfigRegistrarBase<ComplaintKindJournalViewModel, ComplaintKindJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<ComplaintKindJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Объект рекламаций").AddTextRenderer(node => node.ComplaintObject).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Подключаемые отделы").AddTextRenderer(node => node.Subdivisions).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchive).Editing(false).XAlign(0f)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGray : _colorBlack)
				.Finish();
	}
}
