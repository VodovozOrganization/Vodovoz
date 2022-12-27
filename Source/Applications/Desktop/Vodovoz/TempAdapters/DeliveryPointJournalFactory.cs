using System;
using Fias.Client;
using Fias.Client.Cache;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class DeliveryPointJournalFactory : IDeliveryPointJournalFactory
	{
		private DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter;
		
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;

		public DeliveryPointJournalFactory(DeliveryPointJournalFilterViewModel deliveryPointJournalFilter = null)
		{
			_deliveryPointJournalFilter = deliveryPointJournalFilter; 
			IParametersProvider parametersProvider = new ParametersProvider();
			IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(parametersProvider);
			var geoCoderCache = new GeocoderCache(UnitOfWorkFactory.GetDefaultFactory);
			IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken, geoCoderCache);
			_deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(fiasApiClient);
		}

		public void SetDeliveryPointJournalFilterViewModel(DeliveryPointJournalFilterViewModel filter)
		{
			_deliveryPointJournalFilter = filter;
		}

		public IEntityAutocompleteSelectorFactory CreateDeliveryPointAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<DeliveryPointJournalViewModel>(
				typeof(DeliveryPoint), CreateDeliveryPointJournal);
		}

		public IEntityAutocompleteSelectorFactory CreateDeliveryPointByClientAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<DeliveryPointByClientJournalViewModel>(
				typeof(DeliveryPoint), CreateDeliveryPointByClientJournal);
		}

		public DeliveryPointJournalViewModel CreateDeliveryPointJournal()
		{
			var journal = new DeliveryPointJournalViewModel(
				_deliveryPointViewModelFactory,
				_deliveryPointJournalFilter ?? new DeliveryPointJournalFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				hideJournalForOpen: true,
				hideJournalForCreate: true);
			
			return journal;
		}

		public DeliveryPointByClientJournalViewModel CreateDeliveryPointByClientJournal()
		{
			var journal = new DeliveryPointByClientJournalViewModel(
				_deliveryPointViewModelFactory,
				_deliveryPointJournalFilter
				?? throw new ArgumentNullException($"Ожидался фильтр {nameof(_deliveryPointJournalFilter)} с указанным клиентом"),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				hideJournalForOpen: true,
				hideJournalForCreate: true);
			
			return journal;
		}
	}
}
