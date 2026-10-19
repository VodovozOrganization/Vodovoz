using System.Threading;
using System.Threading.Tasks;

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Сервис формирования и создания сделок по отсрочке сети в Битрикс24
	/// </summary>
	public interface IChainStoreDelayDealsCreateService
	{
		/// <summary>
		/// Собирает заказы сетей и тендеров по правилам отчёта "Отсрочка сети" и сохраняет по отправке
		/// на каждый заказ окна, по которому отправки ещё не было, со статусом "Требуется создание сделки"
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество сохранённых отправок</returns>
		Task<int> CollectDeals(CancellationToken cancellationToken);

		/// <summary>
		/// Отправляет в Битрикс24 запросы на создание сделок по отправкам со статусом "Требуется создание сделки".
		/// После успешного создания сохраняет идентификатор сделки Битрикс24, при ошибке - её текст
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		Task SendNotCreatedDeals(CancellationToken cancellationToken);
	}
}
