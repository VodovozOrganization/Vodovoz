using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors;
using WarehouseApi.Contracts.V1.Responses;

namespace WarehouseApi.Library.Services
{
	/// <summary>
	/// Сервис работы с талонами погрузки авто
	/// </summary>
	public interface ICarLoadService
	{
		/// <summary>
		/// Старт погрузки талона погруки
		/// </summary>
		/// <param name="documentId">Номер талона погрузки</param>
		/// <param name="userLogin">Логин пользователя мобильного приложения</param>
		/// <param name="accessToken">Токен доступа к сервису логистических событий</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Результ выполнения запроса и данные талона погрузки</returns>
		Task<RequestProcessingResult<StartLoadResponse>> StartLoad(int documentId, string userLogin, string accessToken, CancellationToken cancellationToken);

		/// <summary>
		/// Получение данных по заказу в талоне погрузки
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Результ выполнения запроса и данные заказа</returns>
		Task<RequestProcessingResult<GetOrderResponse>> GetOrder(int orderId);

		/// <summary>
		/// Добавление кода ЧЗ к товару в талоне погрузки
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="nomenclatureId">Номер номенклатуры</param>
		/// <param name="code">Строка кода ЧЗ</param>
		/// <param name="userLogin">Логин пользователя мобильного приложения</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Результ выполнения запроса и данные строки талона погрузки</returns>
		Task<RequestProcessingResult<AddOrderCodeResponse>> AddOrderCode(int orderId, int nomenclatureId, string code, string userLogin, CancellationToken cancellationToken);

		/// <summary>
		/// Замена кода ЧЗ в талоне погрузки
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="nomenclatureId">Номер номенклатуры</param>
		/// <param name="oldScannedCode">Строка заменяемого(старого) кода ЧЗ</param>
		/// <param name="newScannedCode">Строка нового кода ЧЗ</param>
		/// <param name="userLogin">Логин пользователя мобильного приложения</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Результ выполнения запроса и данные строки талона погрузки</returns>
		Task<RequestProcessingResult<ChangeOrderCodeResponse>> ChangeOrderCode(int orderId, int nomenclatureId, string oldScannedCode, string newScannedCode, string userLogin, CancellationToken cancellationToken);

		/// <summary>
		/// Окончание погрузки талона погруки
		/// </summary>
		/// <param name="documentId">Номер талона погрузки</param>
		/// <param name="userLogin">Логин пользователя мобильного приложения</param>
		/// <param name="accessToken">Токен доступа к сервису логистических событий</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Результ выполнения запроса и данные строки талона погрузки</returns>
		Task<RequestProcessingResult<EndLoadResponse>> EndLoad(int documentId, string userLogin, string accessToken, CancellationToken cancellationToken);
	}
}
