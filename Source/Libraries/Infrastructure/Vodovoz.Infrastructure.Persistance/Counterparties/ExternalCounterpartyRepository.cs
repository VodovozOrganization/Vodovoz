using System;
using System.Collections.Generic;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Nodes;

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
		
		public IEnumerable<ExternalCounterparty> GetActiveExternalCounterpartiesByPhone(IUnitOfWork uow, int phoneId)
		{
			return uow.Session.QueryOver<ExternalCounterparty>()
				.Where(ec => !ec.IsArchive)
				.And(ec => ec.Phone.Id == phoneId)
				.List();
		}
		
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			Phone phoneAlias = null;
			ExternalCounterpartyNode resultAlias = null;
			
			return uow.Session.QueryOver<ExternalCounterparty>()
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.Where(ec => !ec.IsArchive)
				.And(() => phoneAlias.Counterparty.Id == counterpartyId)
				.SelectList(list => list
					.Select(ec => ec.Id).WithAlias(() => resultAlias.Id)
					.Select(ec => ec.ExternalCounterpartyId).WithAlias(() => resultAlias.ExternalCounterpartyId)
					.Select(ec => ec.CounterpartyFrom).WithAlias(() => resultAlias.CounterpartyFrom)
					.Select(() => phoneAlias.Id).WithAlias(() => resultAlias.PhoneId)
					.Select(() => phoneAlias.Number).WithAlias(() => resultAlias.Phone)
				)
				.TransformUsing(Transformers.AliasToBean<ExternalCounterpartyNode>())
				.List<ExternalCounterpartyNode>();
		}
		
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(IUnitOfWork uow, IEnumerable<int> phonesIds)
		{
			Phone phoneAlias = null;
			ExternalCounterpartyNode resultAlias = null;
			
			return uow.Session.QueryOver<ExternalCounterparty>()
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.Where(ec => !ec.IsArchive)
				.AndRestrictionOn(ec => ec.Phone.Id).IsInG(phonesIds)
				.SelectList(list => list
					.Select(ec => ec.Id).WithAlias(() => resultAlias.Id)
					.Select(ec => ec.ExternalCounterpartyId).WithAlias(() => resultAlias.ExternalCounterpartyId)
					.Select(ec => ec.CounterpartyFrom).WithAlias(() => resultAlias.CounterpartyFrom)
					.Select(() => phoneAlias.Id).WithAlias(() => resultAlias.PhoneId)
					.Select(() => phoneAlias.Number).WithAlias(() => resultAlias.Phone)
				)
				.TransformUsing(Transformers.AliasToBean<ExternalCounterpartyNode>())
				.List<ExternalCounterpartyNode>();
		}

		public IList<ExternalCounterparty> GetExternalCounterpartyByEmail(IUnitOfWork uow, int emailId)
		{
			return uow.Session.QueryOver<ExternalCounterparty>()
				.Where(ec => ec.Email.Id == emailId)
				.List();
		}
	}
}
