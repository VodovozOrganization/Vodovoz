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
		protected FluentColumnsConfig<TJournalNode> _fluentColumnsConfig;

		public ColumnsConfigRegistrarBase()
		{
			_fluentColumnsConfig = FluentColumnsConfig<TJournalNode>.Create();

			TreeViewColumnsConfigFactory.Register<TJournalViewModel>(Adapt);
		}

		public abstract IColumnsConfig Configure(FluentColumnsConfig<TJournalNode> config);

		private IColumnsConfig Adapt() => Configure(_fluentColumnsConfig);
	}
}
