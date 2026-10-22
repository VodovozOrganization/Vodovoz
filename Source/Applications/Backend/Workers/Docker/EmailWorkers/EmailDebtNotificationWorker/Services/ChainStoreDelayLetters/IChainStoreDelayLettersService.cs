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
		/// Отбирает заказы для писем и сохраняет историю писем: новые заказы окна и повтор писем, не ушедших из-за отсутствия адреса;
		/// строки "требуется отправка" с наступившей расчётной датой переводит в "не отправлено: наступила расчётная дата платежа"
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество строк, поставленных на отправку</returns>
		Task<int> CollectLetters(CancellationToken cancellationToken);

		/// <summary>
		/// Формирует и отправляет письма по строкам истории в состоянии "требуется отправка"; ошибки отдельных писем
		/// пишутся в журнал и в строку истории, в мониторинг не отправляются
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		Task SendLetters(CancellationToken cancellationToken);
	}
}
