using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Factories
{
	public interface ICashReceiptFactory
	{
		CashReceipt CreateNewCashReceipt(Order order);
		CashReceipt CreateNewCashReceipt(Order order, int? taskId);
	}
}
