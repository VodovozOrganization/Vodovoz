using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис безопасного повторного запуска задачи ЭДО на отправку чека.
	/// </summary>
	public interface IReceiptEdoTaskResendService
	{
		/// <summary>
		/// Проверяет, допускает ли текущее состояние задачи повторный запуск.
		/// </summary>
		/// <param name="receiptTask">Задача ЭДО на отправку чека.</param>
		/// <returns><see langword="true"/>, если задачу можно запустить повторно.</returns>
		bool CanResend(ReceiptEdoTask receiptTask);

		/// <summary>
		/// Публикует событие повторного запуска задачи.
		/// </summary>
		/// <param name="receiptTask">Задача ЭДО на отправку чека.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task PublishResendEventAsync(
			ReceiptEdoTask receiptTask,
			CancellationToken cancellationToken);
	}
}
