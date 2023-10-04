using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DeliveryPointByClientJournalRegistrar : ColumnsConfigRegistrarBase<DeliveryPointByClientJournalViewModel, DeliveryPointByClientJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DeliveryPointByClientJournalNode> config) =>
			config.AddColumn("Адрес")
					.AddTextRenderer(node => node.CompiledAddress)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(1000)
				.AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					c.ForegroundGdk = n.IsActive ? GdkColors.PrimaryText : GdkColors.InsensitiveText;
				})
				.Finish();
	}
}
