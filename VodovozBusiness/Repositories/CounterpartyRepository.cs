using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QSBanks;
using QSContacts;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository
{
	public static class CounterpartyRepository
	{
		public static QueryOver<Counterparty> ActiveClientsQuery ()
		{
			return QueryOver.Of<Counterparty> ()
				.Where (c => !c.IsArchive);
		}

		public static IList<Counterparty> GetCounterpartiesByCode1c (IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<Counterparty> ()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<Counterparty> ();
		}

		public static Counterparty GetCounterpartyByINN (IUnitOfWork uow, string inn)
		{
			if (string.IsNullOrWhiteSpace (inn))
				return null;
			return uow.Session.QueryOver<Counterparty> ()
				.Where (c => c.INN == inn)
				.Take (1)
				.SingleOrDefault ();
		}

		public static IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn) 
		{
			if (string.IsNullOrWhiteSpace (inn))
				return null;
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.INN == inn).List<Counterparty>();
		}

		public static Counterparty GetCounterpartyByAccount (IUnitOfWork uow, string accountNumber)
		{
			if (string.IsNullOrWhiteSpace (accountNumber))
				return null;
			Account accountAlias = null;

			return uow.Session.QueryOver<Counterparty> ()
				.JoinAlias(x => x.Accounts, () => accountAlias)
				.Where (() => accountAlias.Number == accountNumber)
				.Take (1)
				.SingleOrDefault ();
		}

		public static IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Select(Projections.Distinct(Projections.Property<Counterparty>(x => x.SignatoryPost)))
				.List<string>();
		}

		public static IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Select(Projections.Distinct(Projections.Property<Counterparty>(x => x.SignatoryBaseOf)))
				.List<string>();
		}

		public static int GetMaximalInternalID(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>().Select(
				Projections.Max(
					Projections.Property<Counterparty>(x => x.VodovozInternalId)
				)
			).SingleOrDefault<int>();
		}

		public static PaymentType[] GetPaymentTypesForCash() => new PaymentType[] { PaymentType.cash, PaymentType.BeveragesWorld };

		public static PaymentType[] GetPaymentTypesForCashless() => new PaymentType[] { PaymentType.cashless, PaymentType.ByCard, PaymentType.barter, PaymentType.ContractDoc };

		public static bool IsCashPayment(PaymentType payment) => GetPaymentTypesForCash().Contains(payment);

		public static bool IsCashlessPayment(PaymentType payment) => GetPaymentTypesForCashless().Contains(payment);

		public static IList<CounterpartyTo1CNode> GetCounterpartiesWithInnAndAnyContact(IUnitOfWork uow)
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
	}

	public class CounterpartyTo1CNode{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Inn { get; set; }
		public string Phones { get; set; }
		public string EMails { get; set; }
	}
}

