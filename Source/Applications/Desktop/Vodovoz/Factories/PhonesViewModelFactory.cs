using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Factories
{
	public class PhonesViewModelFactory : IPhonesViewModelFactory
	{
		private readonly IPhoneRepository _phoneRepository;
		private readonly RoboatsJournalsFactory _roboatsJournalsFactory;

		public PhonesViewModelFactory(IPhoneRepository phoneRepository)
		{
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));

			RoboatsSettings roboatsSettings = new RoboatsSettings(new SettingsController(UnitOfWorkFactory.GetDefaultFactory));
			RoboatsFileStorageFactory roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			IFileDialogService fileDialogService = new FileDialogService();
			var _roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			_roboatsJournalsFactory = new RoboatsJournalsFactory(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, _roboatsViewModelFactory, nomenclatureJournalFactory);
		}

		public PhonesViewModel CreateNewPhonesViewModel(IUnitOfWork uow) =>
			new PhonesViewModel(
				_phoneRepository,
				uow,
				new ContactParametersProvider(new ParametersProvider()),
				_roboatsJournalsFactory,
				ServicesConfig.CommonServices
			);
	}
}
