using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class CounterpartyRepository : ICounterpartyRepository
	{
		public QueryOver<Counterparty> ActiveClientsQuery()
		{
			return QueryOver.Of<Counterparty>()
				.Where(c => !c.IsArchive);
		}

		public IList<Counterparty> GetCounterpartiesByCode1c(IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<Counterparty>();
		}

		public IList<ClientCameFrom> GetPlacesClientCameFrom(IUnitOfWork uow, bool doNotShowArchive, bool orderByDescending = false)
		{
			var query = uow.Session.QueryOver<ClientCameFrom>();
			
			if(doNotShowArchive)
			{
				query.Where(f => !f.IsArchive);
			}

			return orderByDescending 
				? query.OrderBy(f => f.Name).Desc().List()
				: query.OrderBy(f => f.Name).Asc().List();
		}

		public Counterparty GetCounterpartyByINN(IUnitOfWork uow, string inn)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				return null;
			}

			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.INN == inn)
				.Take(1)
				.SingleOrDefault();
		}

		public IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				return null;
			}

			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.INN == inn).List<Counterparty>();
		}

		public IList<Counterparty> GetNotArchivedCounterpartiesByPhoneNumber(IUnitOfWork uow, string phoneNumber)
		{
			var counterpartiesWithDesiredPhoneNumber = new List<Counterparty>();

			if(string.IsNullOrWhiteSpace(phoneNumber))
			{
				return counterpartiesWithDesiredPhoneNumber;
			}

			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var phoneEntitiesWithDesiredPhoneNumber = uow.Session.QueryOver<Phone>()
				.Where(p => p.Number == phoneNumber)
				.Left.JoinAlias(p => p.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(p => p.DeliveryPoint, () => deliveryPointAlias)
				.List<Phone>();

			counterpartiesWithDesiredPhoneNumber.AddRange(
				phoneEntitiesWithDesiredPhoneNumber
				.Where(p => !p.IsArchive && p.Counterparty != null)
				.Select(p => p.Counterparty));

			var deliveryPointIdsWithDesiredPhoneNumber = phoneEntitiesWithDesiredPhoneNumber
				.Where(p => p.DeliveryPoint != null)
				.Select(p => p.DeliveryPoint?.Id)
				.ToList();

			var counterapartiesWithDesiredPhoneNumberFoundByDeliveryPoint =
				uow.Session.QueryOver(() => deliveryPointAlias)
				.Where(() => deliveryPointAlias.IsActive)
				.Where(Restrictions.In(Projections.Property(() => deliveryPointAlias.Id), deliveryPointIdsWithDesiredPhoneNumber))
				.JoinAlias(d => d.Counterparty, () => counterpartyAlias)
				.Select(Projections.Distinct(Projections.Property<DeliveryPoint>(d => d.Counterparty)))
				.List<Counterparty>();

			counterpartiesWithDesiredPhoneNumber.AddRange(counterapartiesWithDesiredPhoneNumberFoundByDeliveryPoint);

			return counterpartiesWithDesiredPhoneNumber.Distinct().ToList();
		}

		public Counterparty GetCounterpartyByAccount(IUnitOfWork uow, string accountNumber)
		{
			if(string.IsNullOrWhiteSpace(accountNumber))
			{
				return null;
			}

			Account accountAlias = null;

			return uow.Session.QueryOver<Counterparty>()
				.JoinAlias(x => x.Accounts, () => accountAlias)
				.Where(() => accountAlias.Number == accountNumber)
				.Take(1)
				.SingleOrDefault();
		}

		public IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Select(Projections.Distinct(Projections.Property<Counterparty>(x => x.SignatoryPost)))
				.List<string>();
		}

		public IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Select(Projections.Distinct(Projections.Property<Counterparty>(x => x.SignatoryBaseOf)))
				.List<string>();
		}

		public PaymentType[] GetPaymentTypesForCash() => new PaymentType[] { PaymentType.cash };

		public PaymentType[] GetPaymentTypesForCashless() => new PaymentType[] { PaymentType.cashless, PaymentType.ByCard, PaymentType.barter, PaymentType.ContractDoc };

		public bool IsCashPayment(PaymentType payment) => GetPaymentTypesForCash().Contains(payment);

		public bool IsCashlessPayment(PaymentType payment) => GetPaymentTypesForCashless().Contains(payment);

		public IList<CounterpartyTo1CNode> GetCounterpartiesWithInnAndAnyContact(IUnitOfWork uow)
		{
			Email emailAlias = null;
			Counterparty counterpartyAlias = null;
			CounterpartyTo1CNode resultAlias = null;

			var query = uow.Session.QueryOver<Counterparty>(() => counterpartyAlias)
						   .Left.JoinAlias(() => counterpartyAlias.Emails, () => emailAlias)
						   .Where(c => c.INN != "")
						   .SelectList(list => list
									   .SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
									   .Select(x => x.FullName).WithAlias(() => resultAlias.Name)
									   .Select(x => x.INN).WithAlias(() => resultAlias.Inn)
									   .Select(x => x.RingUpPhone).WithAlias(() => resultAlias.Phones)
									   .Select(
										   Projections.SqlFunction(
											   new SQLFunctionTemplate(
												   NHibernateUtil.String,
												   "GROUP_CONCAT(?1 SEPARATOR ';')"
												  ),
											   NHibernateUtil.String,
											   Projections.Property(() => emailAlias.Address))
										  ).WithAlias(() => resultAlias.EMails)
									  )
						   .TransformUsing(Transformers.AliasToBean<CounterpartyTo1CNode>())
						   .List<CounterpartyTo1CNode>();
			return query.Where(x => !String.IsNullOrEmpty(x.EMails) || !String.IsNullOrEmpty(x.Phones)).ToList();
		}

		public IList<Counterparty> GetDealers()
		{
			IList<Counterparty> result;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"Получение списка адресов имеющих фиксированную цену"))
			{
				result = uow.Session.QueryOver<Counterparty>()
				   .Where(c => c.CounterpartyType == CounterpartyType.Dealer)
				   .List<Counterparty>();
			}
			return result;
		}
		
		public Counterparty GetCounterpartyByPersonalAccountIdInEdo(IUnitOfWork uow, string edxClientId)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.PersonalAccountIdInEdo == edxClientId)
				.SingleOrDefault();
		}

		public EdoOperator GetEdoOperatorByCode(IUnitOfWork uow, string edoOperatorCode)
		{
			return uow.Session.QueryOver<EdoOperator>()
				.Where(x => x.Code == edoOperatorCode)
				.SingleOrDefault();
		}
	}
}

