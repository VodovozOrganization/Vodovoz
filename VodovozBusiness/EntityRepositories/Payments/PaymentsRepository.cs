using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Payments
{
	public class PaymentsRepository : IPaymentsRepository
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

		public bool PaymentFromBankClientExists(
			IUnitOfWork uow, DateTime date, int number, string organisationInn, string counterpartyInn, string accountNumber)
		{
			Organization organizationAlias = null;

			var payment = uow.Session.QueryOver<Payment>()
				.JoinAlias(x => x.Organization, () => organizationAlias)
				.Where(p => p.Date == date &&
						p.PaymentNum == number &&
						p.CounterpartyInn == counterpartyInn &&
						p.CounterpartyCurrentAcc == accountNumber)
				.And(() => organizationAlias.INN == organisationInn)
				.SingleOrDefault<Payment>();
			
			return payment != null;
		}

		public decimal GetCounterpartyLastBalance(IUnitOfWork uow, int counterpartyId)
		{
			CashlessMovementOperation cashlessOperationAlias = null;
			Payment paymentAlias = null;
			PaymentItem paymentItemAlias = null;

			var income = uow.Session.QueryOver(() => paymentAlias)
									.Left.JoinAlias(() => paymentAlias.CashlessMovementOperation, () => cashlessOperationAlias)
									.Where(() => paymentAlias.Counterparty.Id == counterpartyId)
									.Select(Projections.Sum(() => cashlessOperationAlias.Income))
									.SingleOrDefault<decimal>();

			var expense = uow.Session.QueryOver(() => paymentItemAlias)
									.Left.JoinAlias(() => paymentItemAlias.Payment, () => paymentAlias)
									.Where(() => paymentAlias.Counterparty.Id == counterpartyId)
									.Select(Projections.Sum(() => paymentItemAlias.Sum))
									.SingleOrDefault<decimal>();

			return income - expense;
		}

		public IList<Payment> GetAllUndistributedPayments(IUnitOfWork uow, IProfitCategoryProvider profitCategoryProvider)
		{
			var undistributedPayments = uow.Session.QueryOver<Payment>()
									.Where(x => x.Status == PaymentState.undistributed)
									.And(x => x.ProfitCategory.Id == profitCategoryProvider.GetDefaultProfitCategory())
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

		public Payment GetRefundPayment(IUnitOfWork uow, int refundedPaymentId)
		{
			var refund = uow.Session.QueryOver<Payment>()
				.Where(p => p.RefundedPayment.Id == refundedPaymentId)
				.SingleOrDefault();
			return refund;
		}
		
		public IList<NotFullyAllocatedPaymentNode> GetAllNotFullyAllocatedPaymentsByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId)
		{
			PaymentItem paymentItemAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;
			NotFullyAllocatedPaymentNode resultAlias = null;

			var payments = uow.Session.QueryOver<Payment>()
				.Left.JoinAlias(p => p.PaymentItems, () => paymentItemAlias)
				.Left.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.SelectList(list =>
					list.SelectGroup(p => p.Id).WithAlias(() => resultAlias.Id)
						.Select(Projections.Sum(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2)"),
							NHibernateUtil.Decimal,
							Projections.Property(() => cashlessMovementOperationAlias.Expense),
							Projections.Constant(0))))
						.WithAlias(() => resultAlias.AllocatedSum)
						.Select(p => p.Total).WithAlias(() => resultAlias.PaymentSum))
				.Where(p => p.Counterparty.Id == counterpartyId)
				.And(p => p.Organization.Id == organizationId)
				.And(Restrictions.GtProperty(
					Projections.Property<Payment>(p => p.Total),
					Projections.Sum(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2)"),
						NHibernateUtil.Decimal,
						Projections.Property(() => cashlessMovementOperationAlias.Expense),
						Projections.Constant(0)))))
				.TransformUsing(Transformers.AliasToBean<NotFullyAllocatedPaymentNode>())
				.List<NotFullyAllocatedPaymentNode>();

			return payments;
		}

		public IQueryOver<Payment, Payment> GetAllUnAllocatedBalances(IUnitOfWork uow, int closingDocumentDeliveryScheduleId)
		{
			UnAllocatedBalancesJournalNode resultAlias = null;
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
				.Select(Projections.Sum<CashlessMovementOperation>(cmo => cmo.Income));
			
			var expense = QueryOver.Of<CashlessMovementOperation>()
				.Where(cmo => cmo.Counterparty.Id == counterpartyAlias.Id)
				.And(cmo => cmo.Organization.Id == organizationAlias.Id)
				.Select(Projections.Sum<CashlessMovementOperation>(cmo => cmo.Expense));

			var balanceProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - ?2"),
					NHibernateUtil.Decimal, new IProjection[] {
						Projections.SubQuery(income),
						Projections.SubQuery(expense)});

			var orderSumProjection = OrderRepository.GetOrderSumProjection(orderItemAlias);
			
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
				.And(() => orderAlias.PaymentType == PaymentType.cashless)
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
				.And(() => orderAlias2.OrderStatus == OrderStatus.Shipped
					|| orderAlias2.OrderStatus == OrderStatus.UnloadingOnStock
					|| orderAlias2.OrderStatus == OrderStatus.Closed)
				.And(() => orderAlias2.PaymentType == PaymentType.cashless)
				.And(() => orderAlias2.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid)
				.And(() => deliveryScheduleAlias2.Id != closingDocumentDeliveryScheduleId)
				.Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense));
			
			var counterpartyDebtProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - IFNULL(?2, ?3)"),
				NHibernateUtil.Decimal, new IProjection[] {
					Projections.SubQuery(totalNotPaidOrders),
					Projections.SubQuery(totalPayPartiallyPaidOrders),
					Projections.Constant(0)});
			
			return query.SelectList(list => list
				.SelectGroup(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
				.SelectGroup(() => organizationAlias.Id).WithAlias(() => resultAlias.OrganizationId)
				.Select(p => counterpartyAlias.INN).WithAlias(() => resultAlias.CounterpartyINN)
				.Select(p => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
				.Select(p =>organizationAlias.Name).WithAlias(() => resultAlias.OrganizationName)
				.Select(balanceProjection).WithAlias(() => resultAlias.CounterpartyBalance)
				.Select(counterpartyDebtProjection).WithAlias(() => resultAlias.CounterpartyDebt))
				.Where(Restrictions.Gt(balanceProjection, 0))
				.And(Restrictions.Gt(counterpartyDebtProjection, 0))
				.OrderBy(balanceProjection).Desc
				.TransformUsing(Transformers.AliasToBean<UnAllocatedBalancesJournalNode>())
				.SetTimeout(180);
		}
	}
	
	public class UnAllocatedBalancesJournalNode : JournalNodeBase
	{
		public int CounterpartyId { get; set; }
		public int OrganizationId { get; set; }
		public string CounterpartyName { get; set; }
		public string CounterpartyINN { get; set; }
		public string OrganizationName { get; set; }
		public decimal CounterpartyBalance { get; set; }
		public decimal CounterpartyDebt { get; set; }
	}

	public class NotFullyAllocatedPaymentNode
	{
		public int Id { get; set; }
		public decimal AllocatedSum { get; set; }
		public decimal PaymentSum { get; set; }
	}
}
