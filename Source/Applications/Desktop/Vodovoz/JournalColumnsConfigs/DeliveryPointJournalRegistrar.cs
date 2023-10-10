using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using VodovozInfrastructure.Extensions;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DeliveryPointJournalRegistrar : ColumnsConfigRegistrarBase<DeliveryPointJournalViewModel, DeliveryPointJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DeliveryPointJournalNode> config) =>
			config.AddColumn("ФИАС").AddTextRenderer(x => x.FoundInFias.ConvertToYesOrEmpty())
				.AddColumn("Испр.").AddTextRenderer(x => x.FixedInFias.ConvertToYesOrEmpty())
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.CompiledAddress)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(1000)
				.AddColumn("Адрес из 1с").AddTextRenderer(x => x.Address1c)
				.AddColumn("Клиент").AddTextRenderer(x => x.Counterparty)
				.AddColumn("Номер").AddTextRenderer(x => x.IdString)
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					c.ForegroundGdk = n.IsActive ? GdkColors.PrimaryText : GdkColors.InsensitiveText;
				})
				.Finish();
	}
}
