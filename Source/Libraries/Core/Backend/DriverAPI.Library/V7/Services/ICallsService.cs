using DriverApi.Contracts.V7.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.V7.Services
{
	/// <summary>
	/// Сервис для работы со звонками
	/// </summary>
	public interface ICallsService
	{
		/// <summary>
		/// Отправляет запрос на совершение звонка через API ВАТС Манго
		/// </summary>
		/// <param name="routeListId">Номер МЛ</param>
		/// <param name="driver">Водитель</param>
		/// <param name="toNumber">Номер телефона, на который нужно позвонить</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат с информацией о запрошенном звонке <see cref="GetCallResponse"/></returns>
		Task<Result<GetCallResponse>> MakeCall(int routeListId, Employee driver, string toNumber, CancellationToken cancellationToken);
	}
}
