using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
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

			RoboatsSettings roboatsSettings = new RoboatsSettings(new ParametersProvider());
			RoboatsFileStorageFactory roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, SingletonErrorReporter.Instance);
			IFileDialogService fileDialogService = new FileDialogService();
			var _roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			var nomenclatureSelectorFactory = new NomenclatureSelectorFactory();
			_roboatsJournalsFactory = new RoboatsJournalsFactory(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, _roboatsViewModelFactory, nomenclatureSelectorFactory);
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
