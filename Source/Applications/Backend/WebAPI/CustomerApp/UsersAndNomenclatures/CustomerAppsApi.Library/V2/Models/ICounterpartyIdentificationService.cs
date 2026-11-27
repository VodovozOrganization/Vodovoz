using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models
{
	/// <summary>
	/// Сервис идентификации клиента
	/// </summary>
	public interface ICounterpartyIdentificationService
	{
		/// <summary>
		/// Поиск контрагента по номеру телефона
		/// </summary>
		/// <param name="request">Запрос с номером телефона</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат поиска</returns>
		Task<CounterpartySearchResultDto> SearchByPhone(
			CounterpartyPhoneSearchDto request,
			CancellationToken cancellationToken = default);
		/// <summary>
		/// Поиск контрагента по ФИО
		/// </summary>
		/// <param name="request">Запрос с ФИО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат поиска</returns>
		Task<CounterpartySearchResultDto> SearchByName(
			CounterpartyNameSearchDto request,
			CancellationToken cancellationToken = default);
		/// <summary>
		/// Поиск контрагента по адресу
		/// </summary>
		/// <param name="request">Запрос с адресом</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат поиска</returns>
		Task<CounterpartySearchResultDto> SearchByAddress(
			CounterpartyAddressSearchDto request,
			CancellationToken cancellationToken = default);
		/// <summary>
		/// Получение истории заказов контрагента
		/// </summary>
		/// <param name="request">Запрос с идентификатором контрагента</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>История заказов</returns>
		Task<CounterpartyOrderHistoryDto> GetOrderHistory(
			CounterpartyOrderHistoryRequestDto request,
			CancellationToken cancellationToken = default);
	}
}
