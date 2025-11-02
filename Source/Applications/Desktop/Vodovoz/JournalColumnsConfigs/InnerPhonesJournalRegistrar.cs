using Gamma.ColumnConfig;
using Vodovoz.Presentation.ViewModels.Employees.Journals;

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
