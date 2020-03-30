using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Operations;
using NHibernate.Criterion;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repositories.Payments
{
	public static class PaymentsRepository
	{
		public static Dictionary<int, decimal> GetAllPaymentsFromTinkoff(IUnitOfWork uow)
		{
			var paymentsList = uow.Session.QueryOver<PaymentFromTinkoff>()
					  .SelectList(list => list
								  .Select(p => p.PaymentNr)
								  .Select(p => p.PaymentRUR)
								 ).List<object[]>();

			return paymentsList.ToDictionary(r => (int)r[0], r => (decimal)r[1]);
		}

		public static IEnumerable<string> GetAllShopsFromTinkoff(IUnitOfWork uow)
		{
			var shops = uow.Session.QueryOver<PaymentFromTinkoff>()
								   .SelectList(list => list.SelectGroup(p => p.Shop))
								   .List<string>();
			return shops;
		}

		public static bool PaymentFromBankClientExists(IUnitOfWork uow, int year, int number, string counterpartyInn, string accountNumber)
		{
			var payment = uow.Session.QueryOver<Payment>()
				.Where(p => p.Date.Year == year &&
						p.PaymentNum == number &&
						p.CounterpartyInn == counterpartyInn &&
						p.CounterpartyCurrentAcc == accountNumber)
				.SingleOrDefault<Payment>();
			return payment != null;
		}

		public static decimal GetCounterpartyLastBalance(IUnitOfWork uow, int counterpartyId)
		{
			CashlessIncomeOperation incomeOperationAlias = null;
			CashlessExpenseOperation expenseOperationAlias = null;

			var income = uow.Session.QueryOver(() => incomeOperationAlias)
									.Where(() => incomeOperationAlias.Counterparty.Id == counterpartyId)
									.Select(Projections.Sum(() => incomeOperationAlias.Sum))
									.SingleOrDefault<decimal>();

			var expense = uow.Session.QueryOver(() => expenseOperationAlias)
									.Where(() => expenseOperationAlias.Counterparty.Id == counterpartyId)
									.Select(Projections.Sum(() => expenseOperationAlias.Sum))
									.SingleOrDefault<decimal>();

			return income - expense;
		}
	}
}
