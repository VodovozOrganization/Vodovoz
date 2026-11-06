using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Отправка одного контрагента в Mindbox и сохранение результата.
	/// </summary>
	public interface IMindboxCounterpartySendService
	{
		/// <summary>
		/// Отправляет контрагента в Mindbox.
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="transactionId">Ключ идемпотентности из сообщения</param>
		/// <param name="retryAttempt">Номер текущей повторной попытки MassTransit</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task SendAsync(
			int counterpartyId,
			Guid transactionId,
			int retryAttempt,
			CancellationToken cancellationToken);
	}
}
