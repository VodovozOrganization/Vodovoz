using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Services;

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
	}
}
