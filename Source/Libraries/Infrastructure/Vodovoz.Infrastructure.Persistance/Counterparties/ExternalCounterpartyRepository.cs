using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class ExternalCounterpartyRepository : IExternalCounterpartyRepository
	{
		public ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, Guid externalCounterpartyId, CounterpartyFrom counterpartyFrom)
		{
			return uow.Session.QueryOver<ExternalCounterparty>()
				.Where(ec => ec.CounterpartyFrom == counterpartyFrom)
				.And(ec => ec.ExternalCounterpartyId == externalCounterpartyId)
				.And(ec => !ec.IsArchive)
				.SingleOrDefault();
		}

		public ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow, Guid externalCounterpartyId, string phoneNumber, CounterpartyFrom counterpartyFrom)
		{
			Phone phoneAlias = null;

			return uow.Session.QueryOver<ExternalCounterparty>()
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.Where(ec => ec.CounterpartyFrom == counterpartyFrom)
				.And(ec => ec.ExternalCounterpartyId == externalCounterpartyId)
				.And(ec => !ec.IsArchive)
				.And(() => phoneAlias.DigitsNumber == phoneNumber)
				.SingleOrDefault();
		}

		public ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, string phoneNumber, CounterpartyFrom counterpartyFrom)
		{
			Phone phoneAlias = null;

			return uow.Session.QueryOver<ExternalCounterparty>()
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.Where(ec => ec.CounterpartyFrom == counterpartyFrom)
				.And(ec => !ec.IsArchive)
				.And(() => phoneAlias.DigitsNumber == phoneNumber)
				.SingleOrDefault();
		}

		public IList<ExternalCounterparty> GetExternalCounterpartyByEmail(IUnitOfWork uow, int emailId)
		{
			return uow.Session.QueryOver<ExternalCounterparty>()
				.Where(ec => ec.Email.Id == emailId)
				.List();
		}

		public bool HasExternalCounterparties(IUnitOfWork uow, int phoneId)
		{
			return uow.Session
				.Query<ExternalCounterparty>()
				.Any(ec => ec.Phone.Id == phoneId);
		}
	}
}
