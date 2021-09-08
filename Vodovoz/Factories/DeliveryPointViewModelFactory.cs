using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QS.Project.Services;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Factories
{
	public class DeliveryPointViewModelFactory : IDeliveryPointViewModelFactory
	{
		public DeliveryPointViewModel GetForOpenDeliveryPointViewModel(int id)
		{
			var controller = new NomenclatureFixedPriceController(
				new NomenclatureFixedPriceFactory(),
				new WaterFixedPricesGenerator(
					new NomenclatureRepository(
						new NomenclatureParametersProvider(
							new ParametersProvider()))));

			var dpViewModel = new DeliveryPointViewModel(
				new UserRepository(),
				new GtkTabsOpener(),
				new PhoneRepository(),
				ContactParametersProvider.Instance,
				new CitiesDataLoader(OsmWorker.GetOsmService()),
				new StreetsDataLoader(OsmWorker.GetOsmService()),
				new HousesDataLoader(OsmWorker.GetOsmService()),
				new NomenclatureSelectorFactory(),
				controller,
				new DeliveryPointRepository(),
				new DeliveryScheduleSelectorFactory(),
				EntityUoWBuilder.ForOpen(id),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices);

			return dpViewModel;
		}

		public DeliveryPointViewModel GetForCreationDeliveryPointViewModel(Counterparty client)
		{
			var controller = new NomenclatureFixedPriceController(
				new NomenclatureFixedPriceFactory(),
				new WaterFixedPricesGenerator(
					new NomenclatureRepository(
						new NomenclatureParametersProvider(
							new ParametersProvider()))));

			var dpViewModel = new DeliveryPointViewModel(
				new UserRepository(),
				new GtkTabsOpener(),
				new PhoneRepository(),
				ContactParametersProvider.Instance,
				new CitiesDataLoader(OsmWorker.GetOsmService()),
				new StreetsDataLoader(OsmWorker.GetOsmService()),
				new HousesDataLoader(OsmWorker.GetOsmService()),
				new NomenclatureSelectorFactory(),
				controller,
				new DeliveryPointRepository(),
				new DeliveryScheduleSelectorFactory(),
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				client);

			return dpViewModel;
		}
	}
}
