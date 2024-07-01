using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.EntityRepositories.Cash.Requests;

namespace Vodovoz.Infrastructure.Persistance.Cash.Requests
{
	internal sealed class CashRequestRepository : ICashRequestRepository
	{
		public IList<CashRequestSumItem> GetCashRequestSumItemsForCashRequest(IUnitOfWork uow, int cashRequestId)
		{
			var result = uow.Session.QueryOver<CashRequestSumItem>()
				.Where(x => x.CashRequest.Id == cashRequestId)
				.List();
			return result;
		}
	}
}
