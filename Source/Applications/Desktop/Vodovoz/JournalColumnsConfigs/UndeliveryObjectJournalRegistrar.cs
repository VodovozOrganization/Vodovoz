using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UndeliveryObjectJournalRegistrar : ColumnsConfigRegistrarBase<UndeliveryObjectJournalViewModel, UndeliveryObjectJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<UndeliveryObjectJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Сопряженные виды").AddTextRenderer(node => node.UndeliveryKinds).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchive).Editing(false).XAlign(0f)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
