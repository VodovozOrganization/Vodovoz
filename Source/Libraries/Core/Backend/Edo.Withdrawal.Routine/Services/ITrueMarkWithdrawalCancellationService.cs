using System.Threading;
using System.Threading.Tasks;

namespace Edo.Withdrawal.Routine.Services
{
	public interface ITrueMarkWithdrawalCancellationService
	{
		/// <summary>
		/// Отправляет в ЧЗ ожидающие документы отмены вывода из оборота.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		Task SendCancellationDocuments(CancellationToken cancellationToken);

		/// <summary>
		/// Публикует заявки ЭДО, для которых ЧЗ успешно отменил вывод кодов из оборота.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		Task PublishReadyResendRequests(CancellationToken cancellationToken);
	}
}
