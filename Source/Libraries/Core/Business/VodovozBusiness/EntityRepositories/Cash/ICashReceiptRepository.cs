using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface ICashReceiptRepository
	{
		bool CashReceiptNeeded(IUnitOfWork uow, int orderId);
		IEnumerable<int> GetOrderIdsForCashReceipt(IUnitOfWork uow);
		IEnumerable<int> GetSelfdeliveryOrderIdsForCashReceipt();
		TrueMarkCashReceiptOrder LoadReceipt(IUnitOfWork uow, int receiptId);
	}
}
