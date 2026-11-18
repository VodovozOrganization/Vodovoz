using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Receipts
{
	/// <summary>
	/// Обработка корректировки чеков при сохранении заказа.
	/// </summary>
	public interface IOrderReceiptCorrectionHandler
	{
		/// <summary>
		/// Проверяет, будет ли при сохранении заказа создан процесс корректировки чека.
		/// Не изменяет данные в БД.
		/// </summary>
		ReceiptCorrectionPreview TryGetCorrectionPreview(IUnitOfWork uow, Order order);

		void TryStartCorrectionProcess(IUnitOfWork uow, Order order);
	}
}
