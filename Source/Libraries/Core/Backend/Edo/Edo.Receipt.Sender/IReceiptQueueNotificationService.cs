using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// Регистрирует уведомления о фискальных документах, находящихся в очереди более суток.
	/// </summary>
	public interface IReceiptQueueNotificationService
	{
		/// <summary>
		/// Проверяет очередь и сохраняет уведомление с временем его регистрации в одной транзакции.
		/// </summary>
		/// <param name="now">Текущее время в той же временной зоне, что и сохранённые статусы кассы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача обработки очереди</returns>
		Task ProcessAsync(DateTime now, CancellationToken cancellationToken);
	}
}
