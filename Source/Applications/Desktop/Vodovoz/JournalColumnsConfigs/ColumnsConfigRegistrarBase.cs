using Gamma.ColumnConfig;
using QS.Journal.GtkUI;
using QS.Project.Journal;

namespace Vodovoz.JournalColumnsConfigs
{
	public abstract class ColumnsConfigRegistrarBase<TJournalViewModel, TJournalNode>
		: IColumnsConfigRegistrar<TJournalViewModel, TJournalNode>
		where TJournalViewModel : JournalViewModelBase
		where TJournalNode : class
	{
		public ColumnsConfigRegistrarBase()
		{
			TreeViewColumnsConfigFactory.Register<TJournalViewModel>(Adapt);
		}

		public abstract IColumnsConfig Configure(FluentColumnsConfig<TJournalNode> config);

		private IColumnsConfig Adapt() => Configure(FluentColumnsConfig<TJournalNode>.Create());
	}
}
