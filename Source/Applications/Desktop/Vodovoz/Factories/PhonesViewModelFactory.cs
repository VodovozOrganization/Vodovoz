using Autofac;
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

namespace Vodovoz.Factories
{
	public class PhonesViewModelFactory : IPhonesViewModelFactory
	{
		private readonly ILifetimeScope _lifetimeScope = MainClass.AppDIContainer.BeginLifetimeScope();
		private readonly IPhoneRepository _phoneRepository;
		private readonly RoboatsJournalsFactory _roboatsJournalsFactory;

		public PhonesViewModelFactory(IPhoneRepository phoneRepository)
		{

			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));

			var roboatsSettings = _lifetimeScope.Resolve<IRoboatsSettings>();
			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			IFileDialogService fileDialogService = new FileDialogService();
			var roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			_roboatsJournalsFactory = new RoboatsJournalsFactory(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, roboatsViewModelFactory, nomenclatureJournalFactory);
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
