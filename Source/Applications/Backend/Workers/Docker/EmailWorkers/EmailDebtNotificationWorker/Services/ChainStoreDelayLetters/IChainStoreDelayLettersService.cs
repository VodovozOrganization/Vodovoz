using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services.ChainStoreDelayLetters
{
	/// <summary>
	/// Сервис писем-напоминаний об оплате по заказам сетей и тендерных клиентов
	/// </summary>
	public interface IChainStoreDelayLettersService
	{
		/// <summary>
		/// Отбирает заказы для писем и сохраняет историю писем: новые заказы окна и повтор писем, не ушедших из-за отсутствия адреса
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество строк, поставленных на отправку</returns>
		Task<int> CollectLetters(CancellationToken cancellationToken);

		/// <summary>
		/// Формирует и отправляет письма по строкам истории в состоянии "требуется отправка"
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество писем с технической ошибкой</returns>
		Task<int> SendLetters(CancellationToken cancellationToken);
	}
}
