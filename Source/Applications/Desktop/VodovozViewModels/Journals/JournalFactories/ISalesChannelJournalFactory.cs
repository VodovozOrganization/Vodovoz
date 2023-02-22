using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISalesChannelJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSalesChannelAutocompleteSelectorFactory();
	}
}