using Mango.Core.Dto.Vpbx.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <summary>
	/// Транспорт запросов к API ВАТС Манго: подписывает тело запроса, отправляет его
	/// и разбирает ответ. Используется сервисами, реализующими конкретные методы API
	/// </summary>
	public interface IMangoVpbxApiClient
	{
		/// <summary>
		/// Выполняет запрос к API ВАТС: подписывает тело запроса, отправляет его
		/// и разбирает ответ, проверяя HTTP-статус и код результата
		/// </summary>
		/// <param name="endpoint">Адрес метода API ВАТС</param>
		/// <param name="request">Тело запроса</param>
		/// <param name="resultCodeRequired">
		/// Возвращает ли метод код результата при успешном выполнении.
		/// Единственный метод, который его не возвращает - запрос списка сотрудников
		/// </param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Разобранный ответ ВАТС</returns>
		Task<TResponse> PostAsync<TRequest, TResponse>(
			string endpoint,
			TRequest request,
			bool resultCodeRequired,
			CancellationToken cancellationToken)
			where TResponse : VpbxResponseBase;
	}
}
