using System;
using System.Threading;
using System.Threading.Tasks;
using Mindbox.Library.Contracts;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Расчёт баланса, доступного списания и начисления бонусов при оформлении заказа
	/// </summary>
	public interface IMindboxAuthorizedCartService
	{
		/// <summary>
		/// Вызывает CalculateAuthorizedCart и разбирает ответ Mindbox
		/// </summary>
		/// <param name="request">Тело запроса</param>
		/// <param name="erpCounterpartyId">Идентификатор клиента в ERP</param>
		/// <param name="externalOrderId">Идентификатор заказа в ИПЗ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат расчёта бонусов</returns>
		Task<MindboxAuthorizedCartResult> CalculateAsync(
			MindboxAuthorizedCartRequest request,
			int erpCounterpartyId,
			Guid externalOrderId,
			CancellationToken cancellationToken);
	}
}
