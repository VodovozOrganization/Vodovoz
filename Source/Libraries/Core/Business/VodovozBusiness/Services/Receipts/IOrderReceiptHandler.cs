using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Receipts
{
	public interface IOrderReceiptHandler
	{
		void RenewOrderCashReceipts(IUnitOfWork uow, Order order);
		bool HasNeededReceipt(int orderId);
	}
}
