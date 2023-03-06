using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		IEnumerable<int> GetNewCashReceiptOrderIds(IUnitOfWork uow);
		int GetCodeErrorsOrdersCount(IUnitOfWork uow);
	}
}
