using System;
using Fias.Service;
using Fias.Service.Loaders;
using QS.DomainModel.UoW;
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
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Factories
{
	public class DeliveryPointViewModelFactory : IDeliveryPointViewModelFactory
	{
		private readonly IFiasApiClient _fiasApiClient;

		public DeliveryPointViewModelFactory(IFiasApiClient fiasApiClient)
		{
			_fiasApiClient = fiasApiClient ?? throw new ArgumentNullException(nameof(fiasApiClient));
		}

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
				new ContactParametersProvider(new ParametersProvider()),
				new CitiesDataLoader(_fiasApiClient),
				new StreetsDataLoader(_fiasApiClient),
				new HousesDataLoader(_fiasApiClient),
				new NomenclatureJournalFactory(),
				controller,
				new DeliveryPointRepository(),
				new DeliveryScheduleSelectorFactory(),
				EntityUoWBuilder.ForOpen(id),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new RoboAtsCounterpartyJournalFactory());

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
				new ContactParametersProvider(new ParametersProvider()),
				new CitiesDataLoader(_fiasApiClient),
				new StreetsDataLoader(_fiasApiClient),
				new HousesDataLoader(_fiasApiClient),
				new NomenclatureJournalFactory(),
				controller,
				new DeliveryPointRepository(),
				new DeliveryScheduleSelectorFactory(),
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new RoboAtsCounterpartyJournalFactory(),
				client);

			return dpViewModel;
		}
	}
}
