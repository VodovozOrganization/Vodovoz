using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Сервис для работы со звонками
	/// </summary>
	public interface ICallsService
	{
		/// <summary>
		/// Отправляет запрос на совершение звонка через вебхук
		/// </summary>
		/// <param name="routeListId">Номер МЛ</param>
		/// <param name="driver">Водитель</param>
		/// <param name="toNumber">Номер телефона, на который нужно позвонить</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат операции со значением id команды на звонок</returns>
		Task<Result<Guid>> MakeWebhookCall(int routeListId, Employee driver, string toNumber, CancellationToken cancellationToken);
	}
}
