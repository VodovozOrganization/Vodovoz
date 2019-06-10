using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;

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
	}
}
