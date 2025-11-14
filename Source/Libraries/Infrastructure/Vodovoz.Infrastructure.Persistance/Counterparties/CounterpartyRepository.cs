using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.HistoryLog.Domain;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozBusiness.Domain.Operations;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class CounterpartyRepository : ICounterpartyRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public CounterpartyRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

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
				.Select(p => new { p.Number, Message = $"Карточка контрагента {p.Counterparty.FullName}" })
				.ToList().Distinct();

			var counterpartiesByDeliveryPointsHavingRequiredNumbers = allPhonesItemsHavingRequiredNumbers
				.Where(p => p.DeliveryPoint != null && p.DeliveryPoint.IsActive && p.DeliveryPoint.Counterparty != null)
				.Select(c => new { c.Number, c.DeliveryPoint })
				.Join(uow.GetAll<Counterparty>(), d => d.DeliveryPoint.Counterparty, c => c, (d, c) => new { d.Number, d.DeliveryPoint, Counterparty = c })
				.Where(dc => dc.Counterparty != null && !dc.Counterparty.IsArchive && dc.Counterparty.Id != currentCounterpartyId)
				.Select(dc => new { dc.Number, Message = $"Точка доставки контрагента \"{dc.Counterparty.FullName}\" по адресу: {dc.DeliveryPoint.ShortAddress}" })
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

			var query = uow.Session.QueryOver(() => counterpartyAlias)
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
			return query.Where(x => !string.IsNullOrEmpty(x.EMails) || !string.IsNullOrEmpty(x.Phones)).ToList();
		}

		public IList<Counterparty> GetDealers()
		{
			IList<Counterparty> result;
			using(var uow = _uowFactory.CreateWithoutRoot($"Получение списка адресов имеющих фиксированную цену"))
			{
				result = uow.Session.QueryOver<Counterparty>()
				   .Where(c => c.CounterpartyType == CounterpartyType.Dealer)
				   .List<Counterparty>();
			}
			return result;
		}

		public Counterparty GetCounterpartyByPersonalAccountIdInEdo(IUnitOfWork uow, string edxClientId)
		{
			CounterpartyEdoOperator edoAccountAlias = null;

			return uow.Session.QueryOver<Counterparty>()
				.JoinAlias(c => c.CounterpartyEdoAccounts, () => edoAccountAlias)
				.Where(() => edoAccountAlias.PersonalAccountIdInEdo == edxClientId)
				.Take(1)
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

		public IQueryable<int> GetLastClassificationCalculationSettingsId(IUnitOfWork uow)
		{
			var query = uow.Session.Query<CounterpartyClassification>()
				.OrderByDescending(c => c.Id)
				.Select(c => c.ClassificationCalculationSettingsId)
				.Take(1);

			return query;
		}

		public IQueryable<CounterpartyClassification> GetLastExistingClassificationsForCounterparties(
			IUnitOfWork uow,
			int lastCalculationSettingsId)
		{
			var query = uow.GetAll<CounterpartyClassification>()
				.Where(c => c.ClassificationCalculationSettingsId == lastCalculationSettingsId)
				.Select(c => new CounterpartyClassification
				{
					Id = c.Id,
					CounterpartyId = c.CounterpartyId,
					ClassificationByBottlesCount = c.ClassificationByBottlesCount,
					ClassificationByOrdersCount = c.ClassificationByOrdersCount,
					BottlesPerMonthAverageCount = c.BottlesPerMonthAverageCount,
					OrdersPerMonthAverageCount = c.OrdersPerMonthAverageCount,
					MoneyTurnoverPerMonthAverageSum = c.MoneyTurnoverPerMonthAverageSum,
					ClassificationCalculationSettingsId = c.ClassificationCalculationSettingsId
				});

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
				join item in uow.GetAll<OrderItem>() on o.Id equals item.Order.Id into items
				from oi in items.DefaultIfEmpty()
				join nomenclature in uow.GetAll<Nomenclature>() on oi.Nomenclature.Id equals nomenclature.Id into nomenclatures
				from n in nomenclatures.DefaultIfEmpty()
				where
					o.DeliveryDate < dateTo
					&& o.DeliveryDate >= dateFrom
					&& o.OrderStatus == OrderStatus.Closed
				group new { Order = o, Item = oi, Nomenclature = n } by new { CleintId = o.Client.Id } into clientsGroups
				select new CounterpartyClassification
				(
					clientsGroups.Key.CleintId,
					clientsGroups.Sum(data =>
							data.Nomenclature != null && data.Item != null
								&& data.Nomenclature.Category == NomenclatureCategory.water
								&& data.Nomenclature.TareVolume == TareVolume.Vol19L
							? data.Item.Count
							: 0),
					clientsGroups.Select(data =>
							data.Order.Id).Distinct().Count(),
					clientsGroups.Sum(data =>
							data.Item != null
							? (data.Item.ActualCount ?? data.Item.Count) * data.Item.Price - data.Item.DiscountMoney
							: 0),
					calculationSettings);

			return query;
		}

		public IQueryable<decimal> GetCounterpartyOrdersActuaSums(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			OrderStatus[] orderStatuses,
			bool isExcludePaidOrders = false,
			DateTime maxDeliveryDate = default)
		{
			var ordersActualSums = from order in unitOfWork.Session.Query<Domain.Orders.Order>()
								   join counterparty in unitOfWork.GetAll<Counterparty>() on order.Client.Id equals counterparty.Id
								   where
								   (!isExcludePaidOrders || order.OrderPaymentStatus != OrderPaymentStatus.Paid)
								   && orderStatuses.Contains(order.OrderStatus)
								   && order.PaymentType == PaymentType.Cashless
								   && counterparty.PersonType == PersonType.legal
								   && counterparty.Id == counterpartyId
								   && (maxDeliveryDate == default || order.DeliveryDate != null && order.DeliveryDate <= maxDeliveryDate)
								   let orderSum = (decimal?)order.OrderItems.Sum(oi => oi.ActualSum) ?? 0m
								   select orderSum;

			return ordersActualSums;
		}

		public IQueryable<CounterpartyCashlessBalanceNode> GetCounterpartiesCashlessBalance(
			IUnitOfWork unitOfWork,
			OrderStatus[] orderStatuses,
			int counterpartyId = default,
			DateTime maxDeliveryDate = default)
		{
			var notPaidOrders =
				from order in unitOfWork.Session.Query<Domain.Orders.Order>()
				join counterparty in unitOfWork.GetAll<Counterparty>() on order.Client.Id equals counterparty.Id

				let orderActualSum = (decimal?)(from orderItem in unitOfWork.Session.Query<OrderItem>()
												where orderItem.Order.Id == order.Id
												select (decimal?)orderItem.ActualSum ?? 0m).Sum() ?? 0m

				let partialPaidOrdersSum = (decimal?)(from paymentItem in unitOfWork.Session.Query<PaymentItem>()
													  join operation in unitOfWork.Session.Query<CashlessMovementOperation>()
													  on paymentItem.CashlessMovementOperation.Id equals operation.Id
													  where
													  paymentItem.Order.Id == order.Id
													  && operation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
													  select (decimal?)operation.Expense ?? 0m).Sum() ?? 0m

				where
				orderStatuses.Contains(order.OrderStatus)
				&& order.OrderPaymentStatus != OrderPaymentStatus.Paid
				&& order.PaymentType == PaymentType.Cashless
				&& counterparty.PersonType == PersonType.legal
				&& (counterpartyId == default || counterparty.Id == counterpartyId)
				&& (maxDeliveryDate == default || order.DeliveryDate != null && order.DeliveryDate <= maxDeliveryDate)
				&& orderActualSum > 0
				select new
				{
					OrderId = order.Id,
					ClientId = counterparty.Id,
					ClientInn = counterparty.INN,
					ClientName = counterparty.FullName,
					OrderItemActualSum = orderActualSum,
					PartialPaidOrdersSum = partialPaidOrdersSum
				};

			var counterpartyCashlessBalance =
				from notPaidOrder in notPaidOrders
				group new { notPaidOrder.ClientInn, notPaidOrder.ClientName, notPaidOrder.OrderId, notPaidOrder.OrderItemActualSum, notPaidOrder.PartialPaidOrdersSum }
				by notPaidOrder.ClientId into ordersByCounterparty

				let cashlessMovementOperationsSum = (decimal?)(from operation in unitOfWork.Session.Query<CashlessMovementOperation>()
															   where
															   operation.Counterparty.Id == ordersByCounterparty.Key
															   && operation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
															   select (decimal?)operation.Income ?? 0m).Sum() ?? 0m

				let paymentsFromBankClientSums = (decimal?)(from paymentItem in unitOfWork.Session.Query<PaymentItem>()
															join payment in unitOfWork.Session.Query<Payment>()
															on paymentItem.Payment.Id equals payment.Id
															where
															payment.Counterparty.Id == ordersByCounterparty.Key
															&& paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
															select (decimal?)paymentItem.Sum ?? 0m).Sum() ?? 0m
				select new CounterpartyCashlessBalanceNode
				{
					CounterpartyId = ordersByCounterparty.Key,
					CounterpartyInn = ordersByCounterparty.Select(o => o.ClientInn).FirstOrDefault() ?? string.Empty,
					CounterpartyName = ordersByCounterparty.Select(o => o.ClientName).FirstOrDefault() ?? string.Empty,
					NotPaidOrdersSum = ordersByCounterparty.Select(o => o.OrderItemActualSum).Sum(),
					PartiallyPaidOrdersSum = ordersByCounterparty.Select(o => o.PartialPaidOrdersSum).Sum(),
					CashlessMovementOperationsSum = cashlessMovementOperationsSum,
					PaymentsFromBankClientSums = paymentsFromBankClientSums
				};

			return counterpartyCashlessBalance;
		}

		public IQueryable<CounterpartyInnName> GetCounterpartyNamesByInn(IUnitOfWork unitOfWork, IList<string> inns)
		{
			var query =
				from counterparty in unitOfWork.Session.Query<Counterparty>()
				where inns.Contains(counterparty.INN)
				select new CounterpartyInnName
				{
					Inn = counterparty.INN,
					Name = counterparty.FullName
				};

			return query;
		}

		public decimal GetTotalDebt(IUnitOfWork unitOfWork, int counterpartyId)
		{
			var orderStatuses = new[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			OrderItem orderItemAlias = null;
			Domain.Orders.Order orderAlias = null;
			PaymentItem paymentItemAlias = null;
			Payment paymentAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;

			var unallocatedIncomeSubquery = QueryOver.Of(() => cashlessMovementOperationAlias)
				.Where(() => cashlessMovementOperationAlias.Counterparty.Id == counterpartyId)
				.Where(() => cashlessMovementOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<CashlessMovementOperation>(x => x.Income));

			var paymentItemsSumSubquery = QueryOver.Of(() => paymentItemAlias)
				.JoinAlias(() => paymentItemAlias.Payment, () => paymentAlias)
				.Where(() => paymentAlias.Counterparty.Id == counterpartyId)
				.Where(() => paymentItemAlias.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<PaymentItem>(x => x.Sum));

			var unallocatedBalanceProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0) - IFNULL(?2, 0)"),
				NHibernateUtil.Decimal,
				Projections.SubQuery(unallocatedIncomeSubquery),
				Projections.SubQuery(paymentItemsSumSubquery)
			);

			var notPaidOrdersSumSubquery = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Order, () => orderAlias)
				.Where(() => orderAlias.Client.Id == counterpartyId)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderStatus.IsIn(orderStatuses))
				.Select(Projections.Sum(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * IFNULL(?2, ?3) - ?4)"),
						NHibernateUtil.Decimal,
						Projections.Property(() => orderItemAlias.Price),
						Projections.Property(() => orderItemAlias.ActualCount),
						Projections.Property(() => orderItemAlias.Count),
						Projections.Property(() => orderItemAlias.DiscountMoney)
					)
				));

			var partialPaidOrdersSumSubquery = QueryOver.Of(() => paymentItemAlias)
				.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.JoinAlias(() => paymentItemAlias.Order, () => orderAlias)
				.Where(() => orderAlias.Client.Id == counterpartyId)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderStatus.IsIn(orderStatuses))
				.Where(() => cashlessMovementOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense));

			var debtProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.Decimal,
					"(IFNULL(?1, 0) - IFNULL(?2, 0) - IFNULL(?3, 0))"
				),
				NHibernateUtil.Decimal,
				Projections.SubQuery(notPaidOrdersSumSubquery),
				unallocatedBalanceProjection,
				Projections.SubQuery(partialPaidOrdersSumSubquery)
			);

			var result = unitOfWork.Session.QueryOver<Counterparty>()
				.Where(c => c.Id == counterpartyId)
				.Select(debtProjection)
				.SingleOrDefault<decimal?>();

			return Math.Round(result ?? 0m, 2);
		}

		public decimal GetDebtByOrganization(IUnitOfWork unitOfWork, int counterpartyId, int organizationId)
		{
			var orderStatuses = new[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			OrderItem orderItemAlias = null;
			Domain.Orders.Order orderAlias = null;
			PaymentItem paymentItemAlias = null;
			CounterpartyContract contractAlias = null;
			Organization organizationAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;

			var notPaidOrdersSumSubquery = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Where(() => orderAlias.Client.Id == counterpartyId)
				.Where(() => organizationAlias.Id == organizationId)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderStatus.IsIn(orderStatuses))
				.Select(Projections.Sum(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * IFNULL(?2, ?3) - ?4)"),
						NHibernateUtil.Decimal,
						Projections.Property(() => orderItemAlias.Price),
						Projections.Property(() => orderItemAlias.ActualCount),
						Projections.Property(() => orderItemAlias.Count),
						Projections.Property(() => orderItemAlias.DiscountMoney)
					)
				));

			var partialPaidOrdersSumSubquery = QueryOver.Of(() => paymentItemAlias)
				.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.JoinAlias(() => paymentItemAlias.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Where(() => orderAlias.Client.Id == counterpartyId)
				.Where(() => organizationAlias.Id == organizationId)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderStatus.IsIn(orderStatuses))
				.Where(() => cashlessMovementOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense));

			var debtProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.Decimal,
					"(IFNULL(?1, 0) - IFNULL(?2, 0))"
				),
				NHibernateUtil.Decimal,
				Projections.SubQuery(notPaidOrdersSumSubquery),
				Projections.SubQuery(partialPaidOrdersSumSubquery)
			);

			var result = unitOfWork.Session.QueryOver<Counterparty>()
				.Where(c => c.Id == counterpartyId)
				.Select(debtProjection)
				.SingleOrDefault<decimal?>();

			return Math.Round(result ?? 0m, 2);
		}

		public IDictionary<int, Email[]> GetCounterpartyEmails(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			uow.Session.Query<Email>()
			.Where(x => x.Counterparty != null && counterparties.Contains(x.Counterparty.Id))
			.GroupBy(x => x.Counterparty.Id)
			.ToDictionary(
				x => x.Key,
				x => x.Distinct().ToArray());

		public IDictionary<int, Phone[]> GetCounterpartyPhones(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			uow.Session.Query<Phone>()
			.Where(x => x.Counterparty.Id != null && counterparties.Contains(x.Counterparty.Id))
			.GroupBy(x => x.Counterparty.Id)
			.ToDictionary(
				x => x.Key,
				x => x.Distinct().ToArray());

		public IDictionary<int, Phone[]> GetCounterpartyOrdersContactPhones(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			(from order in uow.Session.Query<Vodovoz.Domain.Orders.Order>()
			 join phone in uow.Session.Query<Phone>() on order.ContactPhone.Id equals phone.Id
			 where
				order.Client.Id != null
				&& counterparties.Contains(order.Client.Id)
				&& order.ContactPhone.Id != null
			 group phone by order.Client.Id into g
			 select g)
			.ToDictionary(
				x => x.Key,
				x => x.Distinct().ToArray());

		public IList<CounterpartyChangesDto> GetCounterpartyChanges(IUnitOfWork unitOfWork, DateTime fromDate, DateTime toDate)
		{
			var pathNames = new[]
			{
				nameof(Counterparty.CounterpartyType),
				nameof(Counterparty.IsChainStore),
				nameof(Counterparty.CloseDeliveryDebtType),
				nameof(Counterparty.CameFrom),
				nameof(Counterparty.DelayDaysForBuyers),
				nameof(Counterparty.RevenueStatus)
			};

			var counterparties = (from hce in unitOfWork.Session.Query<ChangedEntity>()
				join hc in unitOfWork.Session.Query<FieldChange>() on hce.Id equals hc.Entity.Id
				join counterparty in unitOfWork.Session.Query<Counterparty>() on hce.EntityId equals counterparty.Id
				where
					counterparty.PersonType == PersonType.legal
					&& hce.EntityClassName == nameof(Counterparty)
					&& hce.ChangeTime >= fromDate
					&& hce.ChangeTime <= toDate
					&& pathNames.Contains(hc.Path)
					
				select new CounterpartyChangesDto
				{
					CounterpartyId = counterparty.Id,
					Inn = counterparty.INN,
					Kpp = counterparty.KPP,
					CounterpartyType = counterparty.CounterpartyType,
					IsChainStore = counterparty.IsChainStore,
					CloseDeliveryDebtType = counterparty.CloseDeliveryDebtType,
					CameFrom = counterparty.CameFrom,
					DelayDaysForBuyers = counterparty.DelayDaysForBuyers,
					RevenueStatus = counterparty.RevenueStatus
				})
				.Distinct();

			return counterparties.ToList();
		}
	}
}

