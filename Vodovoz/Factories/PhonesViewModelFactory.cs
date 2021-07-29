using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.ViewModels;
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
			new PhonesViewModel(_phoneRepository, uow, ContactParametersProvider.Instance);
	}
}
