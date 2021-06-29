using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class DeliveryPointJournalFactory: IDeliveryPointJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateDeliveryPointAutocompleteSelectorFactory()
		{
			return new DefaultEntityAutocompleteSelectorFactory<DeliveryPoint, DeliveryPointJournalViewModel, DeliveryPointJournalFilterViewModel>(ServicesConfig.CommonServices);
		}
	}
}
