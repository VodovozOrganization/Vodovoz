using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Factories
{
	public class CashReceiptFactory : ICashReceiptFactory
	{
		public CashReceipt CreateNewCashReceipt(Order order) => CreateCashReceipt(order);

		public CashReceipt CreateNewCashReceipt(Order order, BulkAccountingEdoTask task)
		{
			var receipt = CreateCashReceipt(order);
			receipt.EdoTask = task;
			
			return receipt;
		}

		private CashReceipt CreateCashReceipt(Order order) =>
			new CashReceipt
			{
				Order = order,
				CreateDate = DateTime.Now,
				Status = CashReceiptStatus.New
			};
	}
}
