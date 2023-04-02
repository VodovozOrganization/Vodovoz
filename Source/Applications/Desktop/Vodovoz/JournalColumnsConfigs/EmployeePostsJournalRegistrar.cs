using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EmployeePostsJournalRegistrar : ColumnsConfigRegistrarBase<EmployeePostsJournalViewModel, EmployeePostJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EmployeePostJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название").AddTextRenderer(node => node.EmployeePostName)
				.Finish();
	}
}
