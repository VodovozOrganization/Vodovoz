using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IDeliveryPointJournalFactory
	{
		void SetDeliveryPointJournalFilterViewModel(DeliveryPointJournalFilterViewModel filter);
		IEntityAutocompleteSelectorFactory CreateDeliveryPointAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateDeliveryPointByClientAutocompleteSelectorFactory();
		DeliveryPointJournalViewModel CreateDeliveryPointJournal();
		DeliveryPointByClientJournalViewModel CreateDeliveryPointByClientJournal();
	}
}
