using BitrixApi.Contracts.Dto.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixApi.Library.Services
{
	/// <summary>
	/// Сервис получения заказов для Битрикс
	/// </summary>
	public interface IOrdersService
	{
		/// <summary>
		/// Получение номеров заказов контрагента по номеру телефона,
		/// созданных начиная с указанной даты и не находящихся в отмененных статусах
		/// </summary>
		/// <param name="phone">Номер телефона в формате 7XXXXXXXXXX</param>
		/// <param name="startDate">Дата, начиная с которой ищутся заказы (по дате создания заказа)</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат с Dto ответа, содержащим идентификаторы заказов через запятую</returns>
		Task<Result<GetOrdersResponse>> GetOrdersByPhoneNumberFromDate(string phone, DateTime startDate, CancellationToken cancellationToken);
	}
}
