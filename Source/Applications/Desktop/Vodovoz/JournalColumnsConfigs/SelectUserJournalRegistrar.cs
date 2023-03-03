using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SelectUserJournalRegistrar : ColumnsConfigRegistrarBase<SelectUserJournalViewModel, UserJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<UserJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Имя")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Логин")
					.AddTextRenderer(node => node.Login)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.Deactivated ? _colorDarkGray : _colorBlack)
				.Finish();
	}
}
