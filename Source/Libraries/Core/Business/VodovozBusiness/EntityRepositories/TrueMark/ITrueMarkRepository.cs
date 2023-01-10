using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		IEnumerable<TrueMarkCashReceiptOrder> GetNewCashReceiptOrders(IUnitOfWork uow);
	}
}