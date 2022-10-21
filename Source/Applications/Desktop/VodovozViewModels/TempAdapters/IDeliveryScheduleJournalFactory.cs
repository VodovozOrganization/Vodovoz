using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IDeliveryScheduleJournalFactory : IEntityAutocompleteSelectorFactory
	{
		DeliveryScheduleJournalViewModel CreateJournal(JournalSelectionMode selectionMode);
	}
}
