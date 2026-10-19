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
		/// Перед каждой отправкой, в том числе повторной, заново проверяется оплата заказа: по оплаченным заказам
		/// сделка не создаётся, отправка получает статус "Не отправлена: заказ оплачен до отправки".
		/// После успешного создания сохраняет идентификатор сделки Битрикс24, при ошибке - её текст
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		Task SendNotCreatedDeals(CancellationToken cancellationToken);

		/// <summary>
		/// Переводит в стадию закрытия сделки, заказы которых оплачены.
		/// Сделки в завершённых стадиях и удалённые в Битрикс24 не меняются, их отправкам присваивается
		/// соответствующий статус. При ошибке Битрикс24 её текст сохраняется, закрытие повторится при следующем сборе
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		Task CloseDealsOfPaidOrders(CancellationToken cancellationToken);
	}
}
