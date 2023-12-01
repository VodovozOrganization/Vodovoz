using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class InnerPhonesJournalRegistrar : ColumnsConfigRegistrarBase<InnerPhonesJournalViewModel, InnerPhoneJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<InnerPhoneJournalNode> config) =>
			config
				.AddColumn("Номер")
					.AddTextRenderer(node => node.Number)
				.AddColumn("Описание")
					.AddTextRenderer(node => node.Description)
				.AddColumn("")
				.Finish();
	}


	
}
