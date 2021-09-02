using QS.Project.Journal.EntitySelector;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IDeliveryPointJournalFactory
	{
		void SetDeliveryPointJournalFilterViewModel(DeliveryPointJournalFilterViewModel filter);
		IEntityAutocompleteSelectorFactory CreateDeliveryPointAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateDeliveryPointByClientAutocompleteSelectorFactory();
	}
}
