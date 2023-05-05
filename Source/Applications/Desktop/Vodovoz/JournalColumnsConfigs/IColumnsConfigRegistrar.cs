using Gamma.ColumnConfig;
using QS.Project.Journal;

namespace Vodovoz.JournalColumnsConfigs
{
	public interface IColumnsConfigRegistrar<TJournalViewModel, TJournalNode>
		where TJournalViewModel : JournalViewModelBase
		where TJournalNode : class
	{
		IColumnsConfig Configure(FluentColumnsConfig<TJournalNode> config);
	}
}
