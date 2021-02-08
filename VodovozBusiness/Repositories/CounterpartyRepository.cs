using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using VodovozInfrastructure.Utils;

namespace Vodovoz.Repositories
{
	public static class CounterpartyRepository
	{
		public static QueryOver<Counterparty> ActiveClientsQuery()
		{
			return QueryOver.Of<Counterparty>()
				.Where(c => !c.IsArchive);
		}

		public static IList<Counterparty> GetCounterpartiesByCode1c(IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<Counterparty>();
		}

		public static IList<ClientCameFrom> GetPlacesClientCameFrom(IUnitOfWork uow, bool doNotShowArchive, bool orderByDescending = false)
		{
			var query = uow.Session.QueryOver<ClientCameFrom>();
			if(doNotShowArchive)
				query.Where(f => !f.IsArchive);
			return orderByDescending ? query.OrderBy(f => f.Name).Desc().List() : query.OrderBy(f => f.Name).Asc().List();
		}

		public static Counterparty GetCounterpartyByINN(IUnitOfWork uow, string inn)
		{
			if(string.IsNullOrWhiteSpace(inn))
				return null;
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.INN == inn)
				.Take(1)
				.SingleOrDefault();
		}

		public static IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn)
		{
			if(string.IsNullOrWhiteSpace(inn))
				return null;
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.INN == inn).List<Counterparty>();
		}

		public static Counterparty GetCounterpartyByAccount(IUnitOfWork uow, string accountNumber)
		{
			if(string.IsNullOrWhiteSpace(accountNumber))
				return null;
			Account accountAlias = null;

			return uow.Session.QueryOver<Counterparty>()
				.JoinAlias(x => x.Accounts, () => accountAlias)
				.Where(() => accountAlias.Number == accountNumber)
				.Take(1)
				.SingleOrDefault();
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
		
		
		
		/// <summary>
		/// Возвращает список контрагентов содержащих соответсвующую строку в поле fullName например фамилию имя или отчество
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="partOfName">Часть ФИО</param>
		/// <param name="phoneDigitNumber">Номер телефона в формате 9999999999 (10 цифр)</param>
		/// <exception cref="ArgumentNullException">This exception is thrown if the partOfName == null</exception>
		/// <returns></returns>
		public static IList<Counterparty> GetCounterpartesByPartOfName(IUnitOfWork uow, string partOfName, string phoneDigitNumber)
		{ 
			if (partOfName == null) throw new ArgumentNullException();
			
			Phone phoneAlias = null;
			return uow.Session.QueryOver<Counterparty>()
				.JoinAlias(co => co.Phones, () => phoneAlias)
				.Where(Restrictions.On<Counterparty>(c => c.FullName).IsLike("%" + partOfName + "%"))
				.Where(() => phoneAlias.DigitsNumber == phoneDigitNumber)
				.List();
		}
		
		public static Counterparty GetCounterpartyByBitrixId(IUnitOfWork uow, uint bitrixId) => 
			uow.Session.QueryOver<Counterparty>()
			.Where(x => x.BitrixId == bitrixId)
			.SingleOrDefault();

	}

	public class CounterpartyTo1CNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Inn { get; set; }
		public string Phones { get; set; }
		public string EMails { get; set; }
	}
}

