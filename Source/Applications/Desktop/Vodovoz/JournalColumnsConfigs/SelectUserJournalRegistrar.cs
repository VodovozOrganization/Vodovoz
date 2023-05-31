using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SelectUserJournalRegistrar : ColumnsConfigRegistrarBase<SelectUserJournalViewModel, UserJournalNode>
	{
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
						c.ForegroundGdk = n.Deactivated ? GdkColors.DarkGrayColor : GdkColors.BlackColor)
				.Finish();
	}
}
