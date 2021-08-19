using System;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Loaders;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
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
			return new EntityAutocompleteSelectorFactory<DeliveryPointJournalViewModel>(typeof(DeliveryPoint),
				() => new DeliveryPointJournalViewModel(
					new UserRepository(), new GtkTabsOpener(), new PhoneRepository(),
					ContactParametersProvider.Instance,
					new CitiesDataLoader(OsmWorker.GetOsmService()), new StreetsDataLoader(OsmWorker.GetOsmService()),
					new HousesDataLoader(OsmWorker.GetOsmService()),
					new NomenclatureSelectorFactory(),
					new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory(),
						new WaterFixedPricesGenerator(
							new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())))),
					_deliveryPointJournalFilter ?? new DeliveryPointJournalFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));
		}

		public IEntityAutocompleteSelectorFactory CreateDeliveryPointByClientAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<DeliveryPointByClientJournalViewModel>(typeof(DeliveryPoint),
				() => new DeliveryPointByClientJournalViewModel(
					new UserRepository(), new GtkTabsOpener(), new PhoneRepository(),
					ContactParametersProvider.Instance,
					new CitiesDataLoader(OsmWorker.GetOsmService()), new StreetsDataLoader(OsmWorker.GetOsmService()),
					new HousesDataLoader(OsmWorker.GetOsmService()),
					new NomenclatureSelectorFactory(),
					new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory(),
						new WaterFixedPricesGenerator(
							new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())))),
					_deliveryPointJournalFilter
					?? throw new ArgumentNullException($"Ожидался фильтр {nameof(_deliveryPointJournalFilter)} с указанным клиентом"),
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));
		}
	}
}
