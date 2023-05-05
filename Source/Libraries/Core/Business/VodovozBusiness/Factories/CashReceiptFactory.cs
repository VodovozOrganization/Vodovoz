using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Factories
{
	public class CashReceiptFactory : ICashReceiptFactory
	{
		public CashReceipt CreateNewCashReceipt(Order order)
		{
			return new CashReceipt
            {
            	Order = order,
            	CreateDate = DateTime.Now,
            	Status = CashReceiptStatus.New
            };
		}
	}
}
