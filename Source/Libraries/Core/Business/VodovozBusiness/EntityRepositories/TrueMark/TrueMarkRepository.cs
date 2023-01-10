using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public class TrueMarkRepository : ITrueMarkRepository
	{
		public IEnumerable<TrueMarkCashReceiptOrder> GetNewCashReceiptOrders(IUnitOfWork uow)
		{
			TrueMarkCashReceiptOrder trueMarkCashReceiptOrderAlias = null;
			var result = uow.Session.QueryOver(() => trueMarkCashReceiptOrderAlias)
				.Where(() => trueMarkCashReceiptOrderAlias.Status == TrueMarkCashReceiptOrderStatus.New)
				.List();
			return result;
		}
	}
}
