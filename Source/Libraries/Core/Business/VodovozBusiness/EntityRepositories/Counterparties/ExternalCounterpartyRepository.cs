using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class ExternalCounterpartyRepository : IExternalCounterpartyRepository
	{
		public ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, Guid externalCounterpartyId, CounterpartyFrom counterpartyFrom)
		{
			return uow.Session.QueryOver<ExternalCounterparty>()
				.Where(ec => ec.CounterpartyFrom == counterpartyFrom)
				.And(ec => ec.ExternalCounterpartyId == externalCounterpartyId)
				.SingleOrDefault();
		}
		
		public ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, string phoneNumber, CounterpartyFrom counterpartyFrom)
		{
			Phone phoneAlias = null;
			
			return uow.Session.QueryOver<ExternalCounterparty>()
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.Where(ec => ec.CounterpartyFrom == counterpartyFrom)
				.And(() => phoneAlias.DigitsNumber == phoneNumber)
				.SingleOrDefault();
		}
	}
}
