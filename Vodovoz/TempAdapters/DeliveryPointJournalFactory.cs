using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class DeliveryPointJournalFactory : IDeliveryPointJournalFactory
	{
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter;

		public DeliveryPointJournalFactory(DeliveryPointJournalFilterViewModel deliveryPointJournalFilter = null)
		{
			_deliveryPointJournalFilter = deliveryPointJournalFilter;
		}

		public IEntityAutocompleteSelectorFactory CreateDeliveryPointAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<DeliveryPointJournalViewModel>(typeof(DeliveryPoint), () =>
			{
				return new DeliveryPointJournalViewModel(_deliveryPointJournalFilter ?? new DeliveryPointJournalFilterViewModel(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
