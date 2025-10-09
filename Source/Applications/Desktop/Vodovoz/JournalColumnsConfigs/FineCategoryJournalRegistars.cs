using Gamma.ColumnConfig;
using System.Globalization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.Nodes.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FineCategoryJournalRegistars : ColumnsConfigRegistrarBase<FineCategoryJournalViewModel, FineCategoryJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FineCategoryJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название").AddTextRenderer(node => node.FineCategoryName)
				.AddColumn("")
				.Finish();
	}
}
