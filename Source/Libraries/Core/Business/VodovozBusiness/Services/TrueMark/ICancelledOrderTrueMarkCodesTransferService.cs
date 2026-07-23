using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Сервис переноса отклоненных кодов маркировки из отмененного заказа в другой заказ.
	/// </summary>
	public interface ICancelledOrderTrueMarkCodesTransferService
	{
		/// <summary>
		/// Переносит отклоненные коды маркировки из отмененного заказа в целевой заказ.
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="sourceOrderId">Номер отмененного заказа-источника</param>
		/// <param name="targetOrderId">Номер целевого заказа</param>
		/// <returns>Результат переноса кодов</returns>
		Result<CancelledOrderTrueMarkCodesTransferResult> TransferCodes(
			IUnitOfWork uow,
			int sourceOrderId,
			int targetOrderId);
	}
}
