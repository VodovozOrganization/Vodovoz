using Autofac;
using Fias.Client;
using Fias.Client.Loaders;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.Factories
{
	public class DeliveryPointViewModelFactory : IDeliveryPointViewModelFactory
	{
		private readonly ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly IFiasApiClient _fiasApiClient;
		private readonly IParametersProvider _parametersProvider;
		private readonly IDeliveryScheduleJournalFactory _deliveryScheduleSelectorFactory;
		private readonly RoboatsJournalsFactory _roboatsJournalsFactory;

		public DeliveryPointViewModelFactory(IFiasApiClient fiasApiClient)
		{
			_fiasApiClient = fiasApiClient ?? throw new ArgumentNullException(nameof(fiasApiClient));
			//Необходимо исправить получение всех этих зависимостей из скоупа
			_parametersProvider = new ParametersProvider();
			var roboatsSettings = _lifetimeScope.Resolve<IRoboatsSettings>();
			var roboatsFileStorageFactory =
				new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			IDeliveryScheduleRepository deliveryScheduleRepository = new DeliveryScheduleRepository();
			IFileDialogService fileDialogService = new FileDialogService();
			var roboatsViewModelFactory =
				new RoboatsViewModelFactory(
					roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			_deliveryScheduleSelectorFactory =
				new DeliveryScheduleJournalFactory(
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, deliveryScheduleRepository, roboatsViewModelFactory);

			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			_roboatsJournalsFactory =
				new RoboatsJournalsFactory(
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, roboatsViewModelFactory, nomenclatureJournalFactory);
		}

		public DeliveryPointViewModel GetForOpenDeliveryPointViewModel(int id)
		{
			var controller = new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory());

			var dpViewModel = new DeliveryPointViewModel(
				new UserRepository(),
				new GtkTabsOpener(),
				new PhoneRepository(),
				new ContactParametersProvider(_parametersProvider),
				new CitiesDataLoader(_fiasApiClient),
				new StreetsDataLoader(_fiasApiClient),
				new HousesDataLoader(_fiasApiClient),
				new NomenclatureJournalFactory(),
				controller,
				new DeliveryPointRepository(),
				_deliveryScheduleSelectorFactory,
				EntityUoWBuilder.ForOpen(id),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_roboatsJournalsFactory
				);

			return dpViewModel;
		}

		public DeliveryPointViewModel GetForCreationDeliveryPointViewModel(Counterparty client)
		{
			var controller = new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory());

			var dpViewModel = new DeliveryPointViewModel(
				new UserRepository(),
				new GtkTabsOpener(),
				new PhoneRepository(),
				new ContactParametersProvider(_parametersProvider),
				new CitiesDataLoader(_fiasApiClient),
				new StreetsDataLoader(_fiasApiClient),
				new HousesDataLoader(_fiasApiClient),
				new NomenclatureJournalFactory(),
				controller,
				new DeliveryPointRepository(),
				_deliveryScheduleSelectorFactory,
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_roboatsJournalsFactory,
				client);

			return dpViewModel;
		}
	}
}
