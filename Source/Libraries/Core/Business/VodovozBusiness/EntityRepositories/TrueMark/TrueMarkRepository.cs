using NHibernate.Criterion;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public class TrueMarkRepository : ITrueMarkRepository
	{
		public IEnumerable<int> GetNewCashReceiptOrderIds(IUnitOfWork uow)
		{
			TrueMarkCashReceiptOrder trueMarkCashReceiptOrderAlias = null;
			var result = uow.Session.QueryOver(() => trueMarkCashReceiptOrderAlias)
				.Where(() => trueMarkCashReceiptOrderAlias.Status == TrueMarkCashReceiptOrderStatus.New 
					|| trueMarkCashReceiptOrderAlias.Status == TrueMarkCashReceiptOrderStatus.CodeError)
				.Select(Projections.Id())
				.List<int>();
			return result;
		}

		public int GetCodeErrorsOrdersCount(IUnitOfWork uow)
		{
			var result = uow.Session.QueryOver<TrueMarkCashReceiptOrder>()
				.Where(x => x.Status == TrueMarkCashReceiptOrderStatus.CodeError)
				.ToRowCountQuery()
				.List<int>().First();
			return result;
		}
	}
}
