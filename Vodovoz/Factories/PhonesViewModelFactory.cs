using System;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Factories
{
	public class PhonesViewModelFactory : IPhonesViewModelFactory
	{
		private readonly IPhoneRepository _phoneRepository;

		public PhonesViewModelFactory(IPhoneRepository phoneRepository)
		{
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
		}

		public PhonesViewModel CreateNewPhonesViewModel(IUnitOfWork uow) =>
			new PhonesViewModel(
				_phoneRepository,
				uow,
				new ContactParametersProvider(new ParametersProvider()),
				new RoboAtsCounterpartyJournalFactory(),
				ServicesConfig.CommonServices
			);
	}
}
