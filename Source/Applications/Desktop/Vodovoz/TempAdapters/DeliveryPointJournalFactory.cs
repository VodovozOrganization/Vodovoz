using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class DeliveryPointJournalFactory : IDeliveryPointJournalFactory
	{
		private readonly ILifetimeScope _lifetimeScope;
		private DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter;

		public DeliveryPointJournalFactory(ILifetimeScope lifetimeScope, DeliveryPointJournalFilterViewModel deliveryPointJournalFilter)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_deliveryPointJournalFilter = deliveryPointJournalFilter ?? throw new ArgumentNullException(nameof(deliveryPointJournalFilter)); 
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
				_deliveryPointJournalFilter ?? new DeliveryPointJournalFilterViewModel(),
				_lifetimeScope.Resolve<IUnitOfWorkFactory>(),
				ServicesConfig.CommonServices,
				_lifetimeScope.Resolve<INavigationManager>(),
				hideJournalForOpen: true,
				hideJournalForCreate: true);
			
			return journal;
		}

		public DeliveryPointByClientJournalViewModel CreateDeliveryPointByClientJournal()
		{
			var journal = new DeliveryPointByClientJournalViewModel(
				_deliveryPointJournalFilter
				?? throw new ArgumentNullException($"Ожидался фильтр {nameof(_deliveryPointJournalFilter)} с указанным клиентом"),
				_lifetimeScope.Resolve<IUnitOfWorkFactory>(),
				ServicesConfig.CommonServices,
				_lifetimeScope.Resolve<INavigationManager>(),
				hideJournalForOpen: true,
				hideJournalForCreate: true);
			
			return journal;
		}
	}
}
