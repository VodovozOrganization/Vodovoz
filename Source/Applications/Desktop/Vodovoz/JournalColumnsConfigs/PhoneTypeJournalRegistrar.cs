using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.Journals.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PhoneTypeJournalRegistrar : ColumnsConfigRegistrarBase<PhoneTypeJournalViewModel, PhoneTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PhoneTypeJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Назначение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PhonePurpose.GetEnumTitle())
				.AddColumn("")
				.Finish();
	}
}
