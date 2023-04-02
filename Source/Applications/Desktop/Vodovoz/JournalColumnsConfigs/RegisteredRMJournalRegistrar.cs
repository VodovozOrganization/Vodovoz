using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RegisteredRMJournalRegistrar : ColumnsConfigRegistrarBase<RegisteredRMJournalViewModel, RegisteredRMJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RegisteredRMJournalNode> config) =>
			config.AddColumn("Имя пользователя")
					.AddTextRenderer(node => node.Username)
				.AddColumn("Домен")
					.AddTextRenderer(node => node.Domain)
				.AddColumn("SID пользователя")
					.AddTextRenderer(node => node.SID)
				.AddColumn("")
				.Finish();
	}
}
