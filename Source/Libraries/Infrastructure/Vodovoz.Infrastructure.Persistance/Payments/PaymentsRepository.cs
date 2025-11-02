using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Services;
using VodovozBusiness.Domain.Operations;
using VodovozBusiness.Domain.Payments;
using VodovozBusiness.EntityRepositories.Nodes;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Payments
{
	internal sealed class PaymentsRepository : IPaymentsRepository
	{
		public IList<PaymentByCardOnlineNode> GetPaymentsByTwoMonths(IUnitOfWork uow, DateTime date)
		{
			PaymentByCardOnlineNode resultAlias = null;

			return uow.Session.QueryOver<PaymentByCardOnline>()
				.Where(x => x.DateAndTime >= date.AddMonths(-1))
				.And(x => x.DateAndTime <= date.AddMonths(+1))
				.SelectList(list => list
					.Select(p => p.PaymentNr).WithAlias(() => resultAlias.Number)
					.Select(p => p.DateAndTime).WithAlias(() => resultAlias.Date)
					.Select(p => p.PaymentRUR).WithAlias(() => resultAlias.Sum))
				.TransformUsing(Transformers.AliasToBean<PaymentByCardOnlineNode>())
				.List<PaymentByCardOnlineNode>();
		}

		public IEnumerable<string> GetAllShopsFromTinkoff(IUnitOfWork uow)
		{
			var shops = uow.Session.QueryOver<PaymentByCardOnline>()
								   .SelectList(list => list.SelectGroup(p => p.Shop))
								   .List<string>();
			return shops;
		}

		public bool NotManuallyPaymentFromBankClientExists(
			IUnitOfWork uow,
			DateTime date,
			int number,
			string organisationInn,
			string counterpartyInn,
			string accountNumber,
			decimal sum)
		{
			Organization organizationAlias = null;

			var payment = uow.Session.QueryOver<Payment>()
				.JoinAlias(x => x.Organization, () => organizationAlias)
				.Where(p => p.Date == date)
				.And(p => p.PaymentNum == number)
				.And(p => p.CounterpartyInn == counterpartyInn)
				.And(p => p.CounterpartyCurrentAcc == accountNumber)
				.And(p => p.Total == sum)
				.And(() => organizationAlias.INN == organisationInn)
				.And(p => !p.IsManuallyCreated)
				.SingleOrDefault<Payment>();

			return payment != null;
		}

		public decimal GetCounterpartyLastBalance(IUnitOfWork uow, int counterpartyId, int organizationId)
		{
			CashlessMovementOperation cashlessIncomeOperationAlias = null;
			CashlessMovementOperation cashlessExpenseOperationAlias = null;

			var income = uow.Session.QueryOver(() => cashlessIncomeOperationAlias)
				.Where(() => cashlessIncomeOperationAlias.Counterparty.Id == counterpartyId)
				.And(() => cashlessIncomeOperationAlias.Organization.Id == organizationId)
				.Where(() => cashlessIncomeOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum(() => cashlessIncomeOperationAlias.Income))
				.SingleOrDefault<decimal>();

			var expense = uow.Session.QueryOver(() => cashlessExpenseOperationAlias)
				.Where(() => cashlessExpenseOperationAlias.Counterparty.Id == counterpartyId)
				.And(() => cashlessExpenseOperationAlias.Organization.Id == organizationId)
				.Where(() => cashlessExpenseOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum(() => cashlessExpenseOperationAlias.Expense))
				.SingleOrDefault<decimal>();

			return income - expense;
		}

		public int GetMaxPaymentNumFromManualPayments(IUnitOfWork uow, int counterpartyId, int organizationId)
		{
			return uow.Session.QueryOver<Payment>()
				.Where(p => p.IsManuallyCreated)
				.And(p => p.Counterparty.Id == counterpartyId)
				.And(p => p.Organization.Id == organizationId)
				.And(p => p.Date.Year == DateTime.Today.Year)
				.Select(Projections.Max<Payment>(p => p.PaymentNum))
				.SingleOrDefault<int>();
		}

		public IList<Payment> GetAllUndistributedPayments(IUnitOfWork uow, IPaymentSettings paymentSettings)
		{
			var undistributedPayments = uow.Session.QueryOver<Payment>()
				.Where(x => x.Status == PaymentState.undistributed)
				.And(x => x.ProfitCategory.Id == paymentSettings.DefaultProfitCategory)
				.List();

			return undistributedPayments;
		}

		public IList<Payment> GetAllDistributedPayments(IUnitOfWork uow)
		{
			var distributedPayments = uow.Session.QueryOver<Payment>()
				.Where(x => x.Status == PaymentState.distributed)
				.List();

			return distributedPayments;
		}

		public IEnumerable<Payment> GetNotCancelledRefundedPayments(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<Payment>()
				.Where(p => p.RefundPaymentFromOrderId == orderId)
				.And(p => p.Status != PaymentState.Cancelled)
				.List();
		}

		public IList<NotFullyAllocatedPaymentNode> GetAllNotFullyAllocatedPaymentsByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId, bool allocateCompletedPayments)
		{
			Payment paymentAlias = null;
			PaymentItem paymentItemAlias = null;
			NotFullyAllocatedPaymentNode resultAlias = null;

			var query = uow.Session.QueryOver(() => paymentAlias)
				.Where(p => p.Counterparty.Id == counterpartyId)
				.And(p => p.Organization.Id == organizationId);

			if(allocateCompletedPayments)
			{
				query.And(p => p.Status == PaymentState.completed);
			}
			else
			{
				query.And(p => p.Status != PaymentState.Cancelled);
			}

			var unAllocatedSumProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - IFNULL(?2, ?3)"),
				NHibernateUtil.Decimal,
				Projections.Property(() => paymentAlias.Total),
				Projections.Sum(() => paymentItemAlias.Sum),
				Projections.Constant(0));

			var unAllocatedSum = QueryOver.Of(() => paymentItemAlias)
				.Where(pi => pi.Payment.Id == paymentAlias.Id)
				.And(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(unAllocatedSumProjection);

			var payments = query.SelectList(list =>
					list.SelectGroup(p => p.Id).WithAlias(() => resultAlias.Id)
						.SelectSubQuery(unAllocatedSum).WithAlias(() => resultAlias.UnallocatedSum)
						.Select(p => p.Date).WithAlias(() => resultAlias.PaymentDate))
				.Where(Restrictions.Gt(Projections.SubQuery(unAllocatedSum), 0))
				.TransformUsing(Transformers.AliasToBean<NotFullyAllocatedPaymentNode>())
				.OrderBy(Projections.SubQuery(unAllocatedSum)).Desc
				.OrderBy(p => p.Date).Asc
				.List<NotFullyAllocatedPaymentNode>();

			return payments;
		}

		public IQueryOver<Payment, Payment> GetAllUnallocatedBalances(IUnitOfWork uow, int closingDocumentDeliveryScheduleId)
		{
			UnallocatedBalancesJournalNode resultAlias = null;
			Order orderAlias = null;
			Order orderAlias2 = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization orderOrganizationAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			DeliverySchedule deliveryScheduleAlias2 = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;

			var query = uow.Session.QueryOver<Payment>()
				.Inner.JoinAlias(cmo => cmo.Counterparty, () => counterpartyAlias)
				.Inner.JoinAlias(cmo => cmo.Organization, () => organizationAlias);

			var income = QueryOver.Of<CashlessMovementOperation>()
				.Where(cmo => cmo.Counterparty.Id == counterpartyAlias.Id)
				.And(cmo => cmo.Organization.Id == organizationAlias.Id)
				.And(cmo => cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<CashlessMovementOperation>(cmo => cmo.Income));

			var expense = QueryOver.Of<CashlessMovementOperation>()
				.Where(cmo => cmo.Counterparty.Id == counterpartyAlias.Id)
				.And(cmo => cmo.Organization.Id == organizationAlias.Id)
				.And(cmo => cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<CashlessMovementOperation>(cmo => cmo.Expense));

			var balanceProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - ?2"),
					NHibernateUtil.Decimal,
						Projections.SubQuery(income),
						Projections.SubQuery(expense));

			var orderSumProjection = OrderProjections.GetOrderSumProjection();

			var totalNotPaidOrders = QueryOver.Of(() => orderAlias)
				.Inner.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.Inner.JoinAlias(o => o.Contract, () => counterpartyContractAlias)
				.Inner.JoinAlias(() => counterpartyContractAlias.Organization, () => orderOrganizationAlias)
				.Inner.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
				.Where(() => orderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => orderOrganizationAlias.Id == organizationAlias.Id)
				.And(() => orderAlias.OrderStatus == OrderStatus.Shipped
					|| orderAlias.OrderStatus == OrderStatus.UnloadingOnStock
					|| orderAlias.OrderStatus == OrderStatus.Closed)
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.And(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.And(() => deliveryScheduleAlias.Id != closingDocumentDeliveryScheduleId)
				.Select(orderSumProjection)
				.Where(Restrictions.Gt(orderSumProjection, 0));

			var totalPayPartiallyPaidOrders = QueryOver.Of(() => paymentItemAlias)
				.JoinEntityAlias(() => orderAlias2, () => paymentItemAlias.Order.Id == orderAlias2.Id, JoinType.InnerJoin)
				.Inner.JoinAlias(() => orderAlias2.Contract, () => counterpartyContractAlias)
				.Inner.JoinAlias(() => counterpartyContractAlias.Organization, () => orderOrganizationAlias)
				.Inner.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.Inner.JoinAlias(() => orderAlias2.DeliverySchedule, () => deliveryScheduleAlias2)
				.Where(() => orderAlias2.Client.Id == counterpartyAlias.Id)
				.And(() => orderOrganizationAlias.Id == organizationAlias.Id)
				.And(() => cashlessMovementOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.And(() => orderAlias2.OrderStatus == OrderStatus.Shipped
					|| orderAlias2.OrderStatus == OrderStatus.UnloadingOnStock
					|| orderAlias2.OrderStatus == OrderStatus.Closed)
				.And(() => orderAlias2.PaymentType == PaymentType.Cashless)
				.And(() => orderAlias2.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid)
				.And(() => deliveryScheduleAlias2.Id != closingDocumentDeliveryScheduleId)
				.Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense));

			var counterpartyDebtProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - IFNULL(?2, ?3)"),
				NHibernateUtil.Decimal,
					Projections.SubQuery(totalNotPaidOrders),
					Projections.SubQuery(totalPayPartiallyPaidOrders),
					Projections.Constant(0));

			return query.SelectList(list => list
				.SelectGroup(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
				.SelectGroup(() => organizationAlias.Id).WithAlias(() => resultAlias.OrganizationId)
				.Select(p => counterpartyAlias.INN).WithAlias(() => resultAlias.CounterpartyINN)
				.Select(p => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
				.Select(p => organizationAlias.Name).WithAlias(() => resultAlias.OrganizationName)
				.Select(balanceProjection).WithAlias(() => resultAlias.CounterpartyBalance)
				.Select(counterpartyDebtProjection).WithAlias(() => resultAlias.CounterpartyDebt))
				.Where(Restrictions.Gt(balanceProjection, 0))
				.And(Restrictions.Gt(counterpartyDebtProjection, 0))
				.OrderBy(balanceProjection).Desc
				.TransformUsing(Transformers.AliasToBean<UnallocatedBalancesJournalNode>())
				.SetTimeout(180);
		}

		public bool PaymentFromAvangardExists(IUnitOfWork uow, DateTime paidDate, int orderNum, decimal orderSum)
		{
			var payment = uow.Session.QueryOver<PaymentFromAvangard>()
				.Where(p => p.OrderNum == orderNum)
				.And(p => p.PaidDate == paidDate)
				.And(p => p.TotalSum == orderSum)
				.SingleOrDefault<PaymentFromAvangard>();

			return payment != null;
		}

		public IQueryable<PaymentNode> GetCounterpartyPaymentNodes(IUnitOfWork unitOfWork, int counterpartyId, string counterpartyInn)
		{
			var query = from payment in unitOfWork.Session.Query<Payment>()
						join c in unitOfWork.Session.Query<Counterparty>() on payment.Counterparty.Id equals c.Id into counterparties
						from counterparty in counterparties.DefaultIfEmpty()
						where
						(counterparty.INN == counterpartyInn || counterparty.Id == counterpartyId)
						&& payment.Status != PaymentState.Cancelled
						select new PaymentNode
						{
							PaymentNum = payment.PaymentNum,
							PaymentDate = payment.Date,
							CounterpartyId = counterparty.Id,
							CounterpartyInn = counterparty.INN,
							CounterpartyName = counterparty.Name,
							CounterpartyFullName = counterparty.FullName,
							PayerName = payment.CounterpartyName,
							IsManuallyCreated = payment.IsManuallyCreated,
							PaymentPurpose = payment.PaymentPurpose,
							PaymentSum = payment.Total
						};

			return query;
		}

		public IQueryable<decimal> GetCounterpartyPaymentsSums(IUnitOfWork uow, int counterpartyId, string counterpartyInn)
		{
			var query = from payment in uow.Session.Query<Payment>()
						where
						payment.Status != PaymentState.Cancelled
						&& (payment.Counterparty.Id == counterpartyId || payment.CounterpartyInn == counterpartyInn)
						select payment.Total;

			return query;
		}

		public async Task<IDictionary<int, CounterpartyPaymentsDataNode[]>> GetCounterpatiesPaymentsData(
			IUnitOfWork uow,
			IEnumerable<int> counterpartiesIds,
			int organizationId,
			CancellationToken cancellationToken)
		{
			var query =
				from counterparty in uow.Session.Query<Counterparty>()

				let counterpartyIncomeSum =
				(decimal?)(from cashlessMovementOperations in uow.Session.Query<CashlessMovementOperation>()
						   where
						   cashlessMovementOperations.Counterparty.Id == counterparty.Id
						   && cashlessMovementOperations.CashlessMovementOperationStatus != AllocationStatus.Cancelled
						   && cashlessMovementOperations.Organization.Id == organizationId
						   select cashlessMovementOperations.Income)
						   .Sum() ?? 0

				let counterpartyPaymentItemsSum =
				(decimal?)(from paymentItem in uow.Session.Query<PaymentItem>()
						   join payment in uow.Session.Query<Payment>() on paymentItem.Payment.Id equals payment.Id
						   where
						   payment.Counterparty.Id == counterparty.Id
						   && paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
						   && payment.Organization.Id == organizationId
						   select paymentItem.Sum)
						   .Sum() ?? 0

				let paymentsWriteOffSum =
				(decimal?)(from paymentWriteOff in uow.Session.Query<PaymentWriteOff>()
						   join cashlessMovementOperations in uow.Session.Query<CashlessMovementOperation>()
								on paymentWriteOff.CashlessMovementOperation.Id equals cashlessMovementOperations.Id
						   where
							   paymentWriteOff.CounterpartyId == counterparty.Id
							   && paymentWriteOff.OrganizationId != null
							   && organizationId == paymentWriteOff.OrganizationId.Value
							   && cashlessMovementOperations.CashlessMovementOperationStatus != AllocationStatus.Cancelled
						   select paymentWriteOff.Sum)
						   .Sum() ?? 0

				where counterpartiesIds.Contains(counterparty.Id)

				select new CounterpartyPaymentsDataNode
				{
					CounterpartyId = counterparty.Id,
					CounterpartyName = counterparty.Name,
					CounterpartyInn = counterparty.INN,
					IncomeSum = counterpartyIncomeSum,
					PaymentItemsSum = counterpartyPaymentItemsSum,
					WriteOffSum = paymentsWriteOffSum,
					DelayDaysForCounterparty = counterparty.DelayDaysForBuyers,
					IsLiquidating = counterparty.IsLiquidating,
				};

			var counterpartyPaymentsData =
				(await query.ToListAsync(cancellationToken))
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(
					x => x.Key,
					x => x.ToArray());

			return counterpartyPaymentsData;
		}
	}
}
