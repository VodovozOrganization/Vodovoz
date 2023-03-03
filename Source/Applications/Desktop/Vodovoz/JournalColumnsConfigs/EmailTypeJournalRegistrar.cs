using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.Journals.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EmailTypeJournalRegistrar : ColumnsConfigRegistrarBase<EmailTypeJournalViewModel, EmailTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EmailTypeJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Назначение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.EmailPurpose.GetEnumTitle())
				.AddColumn("")
				.Finish();
	}
}
