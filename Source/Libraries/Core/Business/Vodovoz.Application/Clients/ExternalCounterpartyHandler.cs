using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozBusiness.Services.Clients;

namespace Vodovoz.Application.Clients
{
	public class ExternalCounterpartyHandler : IExternalCounterpartyHandler
	{
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;

		public ExternalCounterpartyHandler(
			IExternalCounterpartyRepository externalCounterpartyRepository
			)
		{
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
		}

		public bool HasExternalCounterparties(IUnitOfWork uow, Phone phone)
		{
			if(phone is null || phone.Id == 0 || phone.Counterparty is null)
			{
				return false;
			}
			
			if(_externalCounterpartyRepository.HasExternalCounterparties(uow, phone.Id))
			{
				return true;
			}
			
			return false;
		}
	}
}
