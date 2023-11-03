using MoreLinq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

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
				.List<Phone>();

			counterpartiesWithDesiredPhoneNumber.AddRange(
				phoneEntitiesWithDesiredPhoneNumber
				.Where(p => !p.IsArchive && p.Counterparty != null && !p.Counterparty.IsArchive)
				.Select(p => p.Counterparty));

			var deliveryPointIdsWithDesiredPhoneNumber = phoneEntitiesWithDesiredPhoneNumber
				.Where(p => !p.IsArchive && p.DeliveryPoint != null)
				.Select(p => p.DeliveryPoint?.Id)
				.ToList();

			var counterapartiesWithDesiredPhoneNumberFoundByDeliveryPoint =
				uow.Session.QueryOver(() => deliveryPointAlias)
				.Where(() => deliveryPointAlias.IsActive)
				.Where(Restrictions.In(Projections.Property(() => deliveryPointAlias.Id), deliveryPointIdsWithDesiredPhoneNumber))
				.JoinAlias(d => d.Counterparty, () => counterpartyAlias)
				.Where(() => !counterpartyAlias.IsArchive)
				.Select(Projections.Distinct(Projections.Property<DeliveryPoint>(d => d.Counterparty)))
				.List<Counterparty>();

			counterpartiesWithDesiredPhoneNumber.AddRange(counterapartiesWithDesiredPhoneNumberFoundByDeliveryPoint);

			return counterpartiesWithDesiredPhoneNumber.Distinct().ToList();
		}

		public IList<string> GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(IUnitOfWork uow, string phoneNumber, int currentCounterpartyId)
		{
			if(string.IsNullOrWhiteSpace(phoneNumber))
			{
				return new List<string>();
			}

			Phone phoneAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var counterpartyDescriptionProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT('Карточка контрагента \"', ?1, '\"')"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.FullName)
			);

			var deliveryPointDescriptionProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT('Точка доставки контрагента \"', ?1, '\" по адресу: ', ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.FullName),
				Projections.Property(() => deliveryPointAlias.ShortAddress)
			);

			var counterpartiesDescription = uow.Session.QueryOver(() => phoneAlias)
				.JoinAlias(() => phoneAlias.Counterparty, () => counterpartyAlias)
				.Where(() => phoneAlias.Number == phoneNumber && !phoneAlias.IsArchive && counterpartyAlias.Id != currentCounterpartyId && !counterpartyAlias.IsArchive)
				.Select(Projections.Distinct(counterpartyDescriptionProjection))
				.List<string>().ToList();

			var counterpartiesDeliveryPointsDescription = uow.Session.QueryOver(() => phoneAlias)
				.JoinAlias(() => phoneAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Counterparty, () => counterpartyAlias)
				.Where(() => phoneAlias.Number == phoneNumber && !phoneAlias.IsArchive && counterpartyAlias.Id != currentCounterpartyId && !counterpartyAlias.IsArchive && deliveryPointAlias.IsActive)
				.Select(Projections.Distinct(deliveryPointDescriptionProjection))
				.List<string>().ToList();

			counterpartiesDescription.AddRange(counterpartiesDeliveryPointsDescription);

			return counterpartiesDescription;
		}

		public Dictionary<string, List<string>> GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(IUnitOfWork uow, List<Phone> phones, int currentCounterpartyId)
		{
			Dictionary<string, List<string>> phoneWithMessages = new Dictionary<string, List<string>>();

			if(phones.Count == 0)
			{
				return phoneWithMessages;
			}

			var allRequiredPhoneNumbers = phones.Select(p => p.DigitsNumber).Distinct();

			var allPhonesItemsHavingRequiredNumbers = uow.GetAll<Phone>().Where(p => allRequiredPhoneNumbers.Contains(p.DigitsNumber) && !p.IsArchive);

			var counterpartiesHavingRequiredNumbers = allPhonesItemsHavingRequiredNumbers
				.Where(p => p.Counterparty != null && p.Counterparty.Id != currentCounterpartyId && !p.Counterparty.IsArchive)
				.Select(p => new { Number = p.Number, Message = $"Карточка контрагента {p.Counterparty.FullName}" })
				.ToList().Distinct();

			var counterpartiesByDeliveryPointsHavingRequiredNumbers = allPhonesItemsHavingRequiredNumbers
				.Where(p => p.DeliveryPoint != null && p.DeliveryPoint.IsActive && p.DeliveryPoint.Counterparty != null)
				.Select(c => new { Number = c.Number, DeliveryPoint = c.DeliveryPoint })
				.Join(uow.GetAll<Counterparty>(), d => d.DeliveryPoint.Counterparty, c => c, (d, c) => new { Number = d.Number, DeliveryPoint = d.DeliveryPoint, Counterparty = c })
				.Where(dc => dc.Counterparty != null && !dc.Counterparty.IsArchive && dc.Counterparty.Id != currentCounterpartyId)
				.Select(dc => new { Number = dc.Number, Message = $"Точка доставки контрагента \"{dc.Counterparty.FullName}\" по адресу: {dc.DeliveryPoint.ShortAddress}" })
				.ToList().Distinct();


			foreach(var phone in counterpartiesHavingRequiredNumbers)
			{
				if(!phoneWithMessages.ContainsKey(phone.Number))
				{
					phoneWithMessages.Add(phone.Number, new List<string> { phone.Message });
				}
				else
				{
					phoneWithMessages[phone.Number].Add(phone.Message);
				}
			}

			foreach(var phone in counterpartiesByDeliveryPointsHavingRequiredNumbers)
			{
				if(!phoneWithMessages.ContainsKey(phone.Number))
				{
					phoneWithMessages.Add(phone.Number, new List<string> { phone.Message });
				}
				else
				{
					phoneWithMessages[phone.Number].Add(phone.Message);
				}
			}

			return phoneWithMessages;
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

		public PaymentType[] GetPaymentTypesForCash() => new PaymentType[] { PaymentType.Cash };

		public PaymentType[] GetPaymentTypesForCashless() => new PaymentType[] { PaymentType.Cashless, PaymentType.PaidOnline, PaymentType.Barter, PaymentType.ContractDocumentation };

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

		public IList<EdoContainer> GetEdoContainersByCounterpartyId(IUnitOfWork uow, int counterpartyId)
		{
			return uow.Session.QueryOver<EdoContainer>()
				.Where(x => x.Counterparty.Id == counterpartyId)
				.OrderBy(x => x.Created).Desc
				.List();
		}

		public IQueryable<DateTime> GetCounterpartyClassificationLastCalculationDate(IUnitOfWork uow)
		{
			var query = uow.Session.Query<CounterpartyClassification>()
				.OrderByDescending(c => c.Id)
				.Select(c => c.ClassificationCalculationDate)
				.Take(1);

			return query;
		}

		public IQueryable<CounterpartyClassification> GetLastExistingClassificationsForCounterparties(
			IUnitOfWork uow,
			DateTime lastCalculationDate)
		{
			var query = uow.GetAll<CounterpartyClassification>()
				.Where(c => c.ClassificationCalculationDate == lastCalculationDate)
				.Select(c => c);

			return query;
		}

		public IQueryable<CounterpartyClassification> CalculateCounterpartyClassifications(
			IUnitOfWork uow,
			CounterpartyClassificationCalculationSettings calculationSettings)
		{
			var creationDate = calculationSettings.SettingsCreationDate;

			var dateFrom = creationDate.Date.AddMonths(-calculationSettings.PeriodInMonths);
			var dateTo = creationDate.Date.AddDays(1);

			var query =
				from o in uow.Session.Query<Domain.Orders.Order>()
				join oi in uow.GetAll<OrderItem>() on o.Id equals oi.Order.Id
				join n in uow.GetAll<Nomenclature>() on oi.Nomenclature.Id equals n.Id
				where
					o.DeliveryDate < dateTo
					&& o.DeliveryDate >= dateFrom
					&& o.OrderStatus == OrderStatus.Closed
				group new { Order = o, Item = oi, Nomenclature = n } by new { CleintId = o.Client.Id } into clientsGroups
				select new CounterpartyClassification
				(
					clientsGroups.Key.CleintId,
					clientsGroups.Sum(data =>
							(data.Nomenclature.Category == NomenclatureCategory.water
								&& data.Nomenclature.TareVolume == TareVolume.Vol19L)
							? data.Item.Count
							: 0),
					clientsGroups.Select(data =>
							data.Order.Id).Distinct().Count(),
					clientsGroups.Sum(data =>
							(data.Item.ActualCount ?? data.Item.Count) * data.Item.Price - data.Item.DiscountMoney),
					creationDate,
					calculationSettings);

			return query;
		}
	}
}

