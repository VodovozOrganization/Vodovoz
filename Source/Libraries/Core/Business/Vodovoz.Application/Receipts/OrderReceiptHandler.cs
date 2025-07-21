using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using VodovozBusiness.Services.Receipts;

namespace Vodovoz.Application.Receipts
{
	public class OrderReceiptHandler : IOrderReceiptHandler
	{
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public OrderReceiptHandler(ICashReceiptRepository cashReceiptRepository)
		{
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}
		
		public void RenewOrderCashReceipts(IUnitOfWork uow, Order order)
		{
			var notNeededReceipts = _cashReceiptRepository.GetReceiptsForOrder(uow, order.Id, CashReceiptStatus.ReceiptNotNeeded);
			
			foreach(var receipt in notNeededReceipts)
			{
				receipt.Status = CashReceiptStatus.New;
				uow.Save(receipt);
			}
		}

		public bool HasNeededReceipt(int orderId)
		{
			return _cashReceiptRepository.HasNeededReceipt(orderId);
		}
	}
}
