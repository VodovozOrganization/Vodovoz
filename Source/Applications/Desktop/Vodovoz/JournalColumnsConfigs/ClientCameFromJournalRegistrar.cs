using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class ClientCameFromJournalRegistrar : ColumnsConfigRegistrarBase<ClientCameFromJournalViewModel, ClientCameFromJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ClientCameFromJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название").AddTextRenderer(n => n.Name)
				.AddColumn("В архиве").AddTextRenderer(n => n.IsArchive.ConvertToYesOrNo())
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
