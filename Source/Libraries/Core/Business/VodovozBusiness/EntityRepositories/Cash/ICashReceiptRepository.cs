using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface ICashReceiptRepository
	{
		bool CashReceiptNeeded(IUnitOfWork uow, int orderId);
		bool CashReceiptNeededForFirstCashSum(IUnitOfWork uow, int orderId);
		IEnumerable<int> GetSelfdeliveryOrderIdsForCashReceipt();
		IEnumerable<CashReceipt> GetCashReceiptsForSend(IUnitOfWork uow, int count);
		CashReceipt LoadReceipt(IUnitOfWork uow, int receiptId);
		IEnumerable<CashReceipt> LoadReceipts(IUnitOfWork uow, IEnumerable<int> receiptId);
		bool HasReceiptBySum(DateTime date, decimal sum);
		int GetCodeErrorsReceiptCount(IUnitOfWork uow);
		IEnumerable<int> GetReceiptIdsForPrepare(int count);
		IEnumerable<int> GetUnfinishedReceiptIds(int count);
	}
}
