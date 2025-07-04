using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Receipts
{
	/// <summary>
	/// Контракт сервиса по работе с чеками в ДВ
	/// </summary>
	public interface IOrderReceiptHandler
	{
		/// <summary>
		/// Обновление чеков
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		void RenewOrderCashReceipts(IUnitOfWork uow, Order order);
		/// <summary>
		/// Проверка необходимости отправки чека
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		/// <returns><c>true</c> - нужен чек, <c>false</c> - чек не нужен</returns>
		bool HasNeededReceipt(int orderId);
	}
}
