using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Factories
{
	public class PhonesViewModelFactory : IPhonesViewModelFactory
	{
		private readonly IPhoneRepository _phoneRepository;
		private IRoboAtsCounterpartyJournalFactory _roboAtsCounterpartyJournalFactory;

		public PhonesViewModelFactory(IPhoneRepository phoneRepository,
			IRoboAtsCounterpartyJournalFactory roboAtsCounterpartyJournalFactory)
		{
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_roboAtsCounterpartyJournalFactory = roboAtsCounterpartyJournalFactory;
		}

		public PhonesViewModel CreateNewPhonesViewModel(IUnitOfWork uow) =>
			new PhonesViewModel(_phoneRepository, uow, ContactParametersProvider.Instance, _roboAtsCounterpartyJournalFactory);
	}
}
